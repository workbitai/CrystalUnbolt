using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrystalUnbolt
{
    public class CrystalLevelController : MonoBehaviour
    {
        private static CrystalLevelController instance;

        [SerializeField] CrystalLevelDatabase database;

        private static bool isLevelLoaded;
        public static bool IsLevelLoaded => isLevelLoaded;

        private static CrystalLevelData level;
        public static CrystalLevelData Level => level;

        private static CrystalStageData stage;
        public static CrystalStageData Stage => stage;

        private static CrystalLevelSave levelSave;

        public static CrystalLevelDatabase Database => instance.database;

        public static int MaxReachedLevelIndex => levelSave.MaxReachedLevelIndex;
        public static int DisplayedLevelIndex => levelSave.DisplayLevelIndex;
        public static bool IsRaycastEnabled { get; set; } = true;

        private static bool firstTimeCompletedLevel = false;

        public static CrystalStageLoader StageLoader { get; private set; }
        private static CrystalSkinsManager skinManager;
        public static CrystalGameTimer GameTimer { get; private set; }

        private static int destroyedPlanksCounter = 0;
        private static int stageId;

        private Coroutine obstructedHolesChecker;

        public static event GameCallback LevelLoaded;
        public static event GameCallback LevelLeft;

        public static bool isRealGameFinish = false;

        private static float _noSelectIdleSeconds = 0f;

        private void Awake()
        {
            instance = this;
        }

        public void Init()
        {
            levelSave = DataManager.GetSaveObject<CrystalLevelSave>("level");

            CrystalRaycastSystem raycastSystem = gameObject.AddComponent<CrystalRaycastSystem>();
            raycastSystem.Init();

            skinManager = new CrystalSkinsManager();
            StageLoader = new CrystalStageLoader(skinManager);
            GameTimer = new CrystalGameTimer();

            CrystalPlankSkinData skinData = (CrystalPlankSkinData)CrystalSkinController.Instance.GetSelectedSkin<CrystalPlanksSkinsDatabase>();
            skinManager.LoadSkin(skinData.PlanksSkinData);

            CrystalSkinController.SkinSelected += SkinSelected;

            GameTimer.OnTimerFinished += OnGameplayTimerFinished;
        }

        private void OnDestroy()
        {
            skinManager.UnloadSkin();
        }

        public static void ClampMaxReachedLevel()
        {
            levelSave.MaxReachedLevelIndex = Mathf.Clamp(levelSave.MaxReachedLevelIndex, 0, Database.AmountOfLevels - 1);
        }

        #region Load/Unload

        public void LoadLevel(int levelIndex, GameCallback onLevelLoaded = null)
        {
            UnloadLevel();
            Debug.Log($"[LevelCheck] Checking => " +
                      $"IsPlayingRandomLevel={levelSave.IsPlayingRandomLevel}, " +
                      $"levelIndex={levelIndex}, " +
                      $"DisplayLevelIndex={levelSave.DisplayLevelIndex}, " +
                      $"RealLevelIndex={levelSave.RealLevelIndex}");

            int realLevelIndex;
            if (levelSave.IsPlayingRandomLevel && levelIndex == levelSave.DisplayLevelIndex && levelSave.RealLevelIndex != -1)
            {
                realLevelIndex = levelSave.RealLevelIndex;
                Debug.Log("realLevelIndex  =>  " + realLevelIndex);
            }
            else
            {
                realLevelIndex = database.GetRandomLevelIndex(levelIndex, levelSave.LastPlayerLevelIndex, false);
                Debug.Log("realLevelIndex GetRandomLevelIndex  =>  " + realLevelIndex);

                levelSave.LastPlayerLevelIndex = realLevelIndex;
                levelSave.RealLevelIndex = realLevelIndex;

                if (realLevelIndex != levelIndex)
                {
                    levelSave.IsPlayingRandomLevel = true;
                }
            }

            levelSave.DisplayLevelIndex = levelIndex;
            firstTimeCompletedLevel = false;

            stageId = 0;

            Debug.Log("realLevelIndex  =>  " + realLevelIndex);
            level = database.GetLevel(realLevelIndex);
            Debug.Log("Get Level =>  " + level);
            stage = level.Stages[stageId];

            StageLoader.LoadStage(stage);
            destroyedPlanksCounter = 0;
            InitTimer();

            CrystalUIGame gameUI = ScreenManager.GetPage<CrystalUIGame>();
            gameUI.PowerUpsUIController.OnLevelStarted(levelIndex + 1);
            gameUI.UpdateLevelNumber(levelIndex + 1);

            gameUI.SpawnLevelStages(level.Stages.Count);
            gameUI.SetActiveStage(0);

            CrystalRaycastSystem.Disable();

            isLevelLoaded = true;
            isRealGameFinish = false;

            CrystalRaycastSystem.Enable();

            onLevelLoaded?.Invoke();
            LevelLoaded?.Invoke();

            _noSelectIdleSeconds = 0f;

            obstructedHolesChecker = StartCoroutine(ObstructedHolesChecker());

            IsRaycastEnabled = true;

            SavePresets.CreateSave("Level " + (levelIndex + 1).ToString("000"), "Levels", "Level " + (levelIndex + 1));
        }

        public static void UnloadLevel()
        {
            if (instance != null && instance.obstructedHolesChecker != null)
            {
                instance.StopCoroutine(instance.obstructedHolesChecker);
                instance.obstructedHolesChecker = null;
            }

            _noSelectIdleSeconds = 0f;

            if (StageLoader != null && StageLoader.BaseHoles != null)
            {
                for (int i = 0; i < StageLoader.BaseHoles.Count; i++)
                {
                    ToggleHole(StageLoader.BaseHoles[i], false);
                }
            }

            StageLoader.UnloadStage(false);
            CrystalPUController.ResetBehaviors();

            LevelLeft?.Invoke();
        }

        public void ReloadStage()
        {
            Debug.Log("ReloadStage");
            StageLoader.UnloadStage(false);
            CrystalPUController.ResetBehaviors();

            stage = level.Stages[stageId];

            destroyedPlanksCounter = 0;

            Tween.NextFrame(() =>
            {
                StageLoader.LoadStage(stage);

                ScreenManager.GetPage<CrystalUIGame>().SetActiveStage(stageId);

                InitTimer();
            });
        }

        private void SkinSelected(ISkinData skinData)
        {
            if (skinData is CrystalPlankSkinData data)
            {
                skinManager.UnloadSkin();
                skinManager.LoadSkin(data.PlanksSkinData);
            }
        }

        #endregion

        #region Gameplay

        public static void ProcessClick(List<IClickableObject> clickableObjects2D)
        {
            int holesCounter = 0;
            int planksCounter = 0;

            List<CrystalHoleController> holes = new List<CrystalHoleController>();

            for (int i = 0; i < clickableObjects2D.Count; i++)
            {
                IClickableObject clickableObject = clickableObjects2D[i];

                if (clickableObject is CrystalHoleController hole)
                {
                    holesCounter++;
                    holes.Add(hole);
                }
                else if (clickableObject is CrystalPlankController)
                {
                    planksCounter++;
                }
            }

            if (holes.Count == planksCounter + 1)
            {
                bool alligned = true;

                if (holes.Count > 1)
                {
                    for (int i = 1; i < holes.Count; i++)
                    {
                        if (Vector2.Distance(holes[0].transform.position, holes[i].transform.position) > 0.1f)
                        {
                            alligned = false;
                            break;
                        }
                    }
                }

                if (CrystalScrewController.SelectedScrew != null && alligned)
                {
                    CrystalScrewController.SelectedScrew.ChangeHoles(holes);
                }
            }
        }

        public static void UpdateDestroyedPlanks()
        {
            destroyedPlanksCounter++;
            if (destroyedPlanksCounter == StageLoader.AmountOfPlanks)
            {
                isRealGameFinish = true;
                ScreenManager.GetPage<CrystalUIGame>()?.SetReplayButtonInteractable(false);
                CrystalGameOverArranger.GameOverSequence();
            }
        }

        public static void OnPlankStartFalling()
        {
            if (StageLoader == null) return;
            if (destroyedPlanksCounter == StageLoader.AmountOfPlanks - 1)
            {
                ScreenManager.GetPage<CrystalUIGame>()?.SetReplayButtonInteractable(false);
            }
        }

        public static void OnPuzzleCompleted()
        {
            stageId++;

            GameTimer.Reset();

            if (stageId >= level.Stages.Count)
            {
                levelSave.IsPlayingRandomLevel = false;
                levelSave.RealLevelIndex = -1;
                Debug.Log(" levelSave.DisplayLevelIndex  =>  " + levelSave.DisplayLevelIndex);
                levelSave.DisplayLevelIndex++;
                Debug.Log(" levelSave.DisplayLevelIndex  =>  " + levelSave.DisplayLevelIndex);

                if (levelSave.DisplayLevelIndex > levelSave.MaxReachedLevelIndex)
                {
                    levelSave.MaxReachedLevelIndex = levelSave.DisplayLevelIndex;
                    firstTimeCompletedLevel = true;
                    CrystalCloudProgressSync.PushLocalToCloud();
                }

                CrystalPUPrice[] powerUpsReward = GetPUReward();
                if (!powerUpsReward.IsNullOrEmpty())
                {
                    foreach (CrystalPUPrice reward in powerUpsReward)
                    {
                        CrystalPUController.AddPowerUp(reward.PowerUpType, reward.Amount);
                    }
                }

                CrystalGameManager.OnLevelCompleted();
            }
            else
            {
                CrystalUIGame gameUI = ScreenManager.GetPage<CrystalUIGame>();
                gameUI.ShowStageComplete(() =>
                {
                    StageLoader.UnloadStage(true);

                    stage = level.Stages[stageId];

                    destroyedPlanksCounter = 0;

                    Tween.NextFrame(() =>
                    {
                        StageLoader.LoadStage(stage);
                        gameUI.SetActiveStage(stageId);
                        InitTimer();
                    });

                    SoundManager.PlaySound(SoundManager.AudioClips.levelComplete);
                });
            }
        }


        static readonly Dictionary<CrystalBaseHole, Coroutine> _holeBlinkCo = new();
        // Theme colors for available holes (matches provided blue swatch #00ADFF)
        static readonly Color holeGlowColor = new Color(0f / 255f, 173f / 255f, 1f, 1f);      // base glow
        static readonly Color rippleColor   = new Color(102f / 255f, 224f / 255f, 1f, 1f);    // lighter ripple

        static void ToggleHole(CrystalBaseHole bh, bool on)
        {
            if (instance == null || bh == null || bh.hole == null) return;

            if (_holeBlinkCo.TryGetValue(bh, out var co) && co != null)
            {
                instance.StopCoroutine(co);
                _holeBlinkCo.Remove(bh);
            }

            SpriteRenderer sr = bh.hole; 
            SpriteRenderer ripple = bh.GetOrCreateRippleRenderer();

            if (bh.IsLocked)
            {
                sr.color = Color.white;
                sr.gameObject.SetActive(false);
                if (ripple != null) ripple.gameObject.SetActive(false);
                return;
            }

            if (!on)
            {
                sr.color = Color.white;
                bh.ResetDataHole(true);
                sr.gameObject.SetActive(false);

                if (ripple != null)
                {
                    ripple.color = new Color(1f, 1f, 1f, 0f);
                    ripple.transform.localScale = Vector3.one;
                    ripple.gameObject.SetActive(false);
                }
                return;
            }

            sr.gameObject.SetActive(true);
            sr.enabled = true;
            sr.color = Color.white;
            if (ripple != null)
            {
                ripple.gameObject.SetActive(true);
                ripple.transform.localScale = Vector3.one * 0.6f;
                ripple.color = new Color(rippleColor.r, rippleColor.g, rippleColor.b, 0.45f);
            }

            _holeBlinkCo[bh] = instance.StartCoroutine(HoleBlinkRoutine(sr, ripple));
        }

        static IEnumerator HoleBlinkRoutine(SpriteRenderer sr, SpriteRenderer ripple)
        {
            const float pulseDuration = 0.9f;
            const float colorPulseSpeed = 2.2f;
            float timer = 0f;

            while (true)
            {
                timer += Time.unscaledDeltaTime;

                float colorT = (Mathf.Sin(timer * colorPulseSpeed) + 1f) * 0.5f;
                sr.color = Color.Lerp(Color.white, holeGlowColor, colorT * 0.5f);

                if (ripple != null)
                {
                    float rippleT = (timer % pulseDuration) / pulseDuration;
                    float eased = rippleT * rippleT * (3f - 2f * rippleT);

                    float scale = Mathf.Lerp(0.65f, 1.55f, eased);
                    float alpha = Mathf.Lerp(0.45f, 0f, eased);

                    ripple.transform.localScale = Vector3.one * scale;
                    ripple.color = new Color(1f, 1f, 1f, alpha);
                }

                yield return null;
            }
        }


       
        private static bool ShouldDisableHoleBlinking()
        {
            string disableReason = "";

            try
            {
                CrystalTutorialID[] tutorialTypes = { 
                    CrystalTutorialID.FirstLevel, 
                    CrystalTutorialID.PURemoveScrew, 
                    CrystalTutorialID.PURemovePlank, 
                    CrystalTutorialID.PUExtraHole, 
                    CrystalTutorialID.CrystalPUTimer 
                };

                foreach (CrystalTutorialID tutorialType in tutorialTypes)
                {
                    CrystalITutorial tutorial = CrystalTutorialController.GetTutorial(tutorialType);
                    if (tutorial != null && tutorial.IsActive && !tutorial.IsFinished)
                    {
                        disableReason = $"Tutorial active: {tutorialType}";
                        Debug.Log($"[LevelController] Hole blinking disabled: {disableReason}");
                        return true; 
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[LevelController] Tutorial check failed: {e.Message}");
            }

            if (CrystalGameOverArranger.IsPuzzleModeActive)
            {
                disableReason = "Game over puzzle mode active";
              //  Debug.Log($"[LevelController] Hole blinking disabled: {disableReason}");
                return true; 
            }

            if (isRealGameFinish)
            {
                disableReason = "Game over sequence playing";
              //  Debug.Log($"[LevelController] Hole blinking disabled: {disableReason}");
                return true; 
            }

            if (!isLevelLoaded)
            {
                disableReason = "Level not fully loaded";
               // Debug.Log($"[LevelController] Hole blinking disabled: {disableReason}");
                return true; 
            }

            if (!IsRaycastEnabled)
            {
                disableReason = "Raycast disabled";
              //  Debug.Log($"[LevelController] Hole blinking disabled: {disableReason}");
                return true; 
            }

            if (DisplayedLevelIndex == 0)
            {
                if (GameTimer != null && !GameTimer.IsActive)
                {
                    disableReason = "Level 1 with paused timer (likely tutorial)";
                  //  Debug.Log($"[LevelController] Hole blinking disabled: {disableReason}");
                    return true; 
                }
            }

            return false;
        }

        private static IEnumerator ObstructedHolesChecker()
        {
            CrystalUIGame uiGame = ScreenManager.GetPage<CrystalUIGame>();
            WaitForSeconds wait = new WaitForSeconds(0.5f); 

            int counter = 0;
            while (true)
            {
                yield return wait;

                if (!CrystalGameManager.IsGameActive) continue;

                bool shouldDisableBlinking = ShouldDisableHoleBlinking();

                bool anyScrewSelected = (CrystalScrewController.SelectedScrew != null);
                if (anyScrewSelected)
                {
                    _noSelectIdleSeconds = 0f;
                }
                else
                {
                    _noSelectIdleSeconds += 0.5f; 
                }

                bool inBlinkWindow = (!anyScrewSelected) && (_noSelectIdleSeconds >= 3f && _noSelectIdleSeconds < 5f) && !shouldDisableBlinking;

                bool hasMoves = false;

                for (int i = 0; i < StageLoader.BaseHoles.Count; i++)
                {
                    CrystalBaseHole hole = StageLoader.BaseHoles[i];
                    if (hole == null) continue;

                    bool canPlayHere = false;

                    if (!hole.IsActive)
                    {
                        List<IClickableObject> clickableObjects2D =
                            CrystalRaycastSystem.HasOverlapCircle2D(hole.Position, hole.PhysicsRadius, 2560);

                        int holesCounter = 0;
                        int planksCounter = 0;

                        for (int j = 0; j < clickableObjects2D.Count; j++)
                        {
                            IClickableObject obj = clickableObjects2D[j];

                            if (obj is CrystalPlankHole plankHole)
                            {
                                if (Vector2.Distance(hole.transform.position, plankHole.transform.position) > 0.1f)
                                {
                                    holesCounter = -1;
                                    break;
                                }

                                holesCounter++;
                            }
                            else if (obj is CrystalPlankController)
                            {
                                planksCounter++;
                            }
                        }

                        if (holesCounter == planksCounter)
                        {
                            canPlayHere = true;
                            hasMoves = true;
                        }
                    }

                    if (canPlayHere && inBlinkWindow) ToggleHole(hole, true);
                    else ToggleHole(hole, false);
                }

                if (hasMoves)
                {
                    uiGame.HideNoMoreMovesIndicator();
                    counter = 0;
                }
                else if (counter == 5)
                {
                    uiGame.ShowNoMoreMovesIndicator();
                    counter++;
                }
                else
                {
                    counter++;
                }

            }
        }

        public static int GetCurrentLevelReward()
        {
            int reward = 0;

            if (level != null && level.OverrideLevelReward.Enabled)
            {
                reward = level.OverrideLevelReward.Value;
            }
            else
            {
                reward = CrystalGameManager.Data.CoinsRewardPerLevel;
            }

            if (!firstTimeCompletedLevel)
            {
                reward = Mathf.RoundToInt(reward * (CrystalGameManager.Data.ReplayingLevelRewardPercent.Handle(0) / 100.0f) / 10) * 10;
            }

            return reward;
        }
        #endregion

        #region PU

        public static void PlaceAdditionalBaseHole(Vector3 position)
        {
            StageLoader.PlaceAdditionalBaseHole(position);
        }

        public static CrystalPUPrice[] GetPUReward()
        {
            if (level == null) return null;
            if (!firstTimeCompletedLevel) return null;

            return level.PowerUpsReward;
        }

        #endregion

        #region Timer

        private static void InitTimer()
        {
            int levelNumber = DisplayedLevelIndex + 1; // 1-based level number
            Debug.Log($"[Timer] InitTimer called - DisplayedLevelIndex: {DisplayedLevelIndex}, Level Number: {levelNumber}");
            
            // Skip timer for levels 1-10 (DisplayedLevelIndex 0-9)
            if (DisplayedLevelIndex < 10)
            {
                Debug.Log($"[Timer] Skipping timer for level {levelNumber} (levels 1-10 have no timer)");
                return;
            }

            if (CrystalGameManager.Data.GameplayTimerEnabled)
            {
                float time = CrystalGameManager.Data.GameplayTimerValue;

                if (stage.TimerOverrideEnabled) time = stage.TimerOverride;

                GameTimer.SetMaxTime(time);
                
                // For level 11 ONLY (DisplayedLevelIndex == 10), don't start the timer yet - it will be started after popup
                if (DisplayedLevelIndex == 10 && levelNumber == 11) // Level 11 (0-based index 10)
                {
                    Debug.Log($"[Timer] Level 11 detected (index {DisplayedLevelIndex}) - Timer initialized but NOT started, waiting for popup");
                    // Don't start yet - will be started after popup in CrystalUIGame
                }
                else
                {
                    GameTimer.Start();
                    Debug.Log($"[Timer] Timer started for level {levelNumber} (index {DisplayedLevelIndex})");
                }
            }
            else
            {
                Debug.Log($"[Timer] Timer is disabled in game settings");
            }
        }

        private void Update()
        {
            GameTimer.Update();
        }

        private static void OnGameplayTimerFinished()
        {
            IsRaycastEnabled = false;

            CrystalGameManager.OnLevelFailed();

            SoundManager.PlaySound(SoundManager.AudioClips.levelFailed);
        }

        #endregion
    }
}
