using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using CrystalUnbolt.Map;

namespace CrystalUnbolt
{
    public class CrystalUIGame : BaseScreen
    {
        [BoxGroup("Safe Area", "Safe Area")]
        [SerializeField] RectTransform safeAreaRectTransform;
        [BoxGroup("Safe Area")]
        [SerializeField] CrystalCurrencyUIPanelSimple coinsPanel;
        [BoxGroup("Safe Area")]
        [SerializeField] CrystalUILevelNumberText levelNumberText;
        [BoxGroup("Safe Area")]
        [SerializeField] CrystalStagePanel CrystalStagePanel;
        [BoxGroup("Safe Area")]
        [SerializeField] CrystalTimerVisualiser gameplayTimer;
        [BoxGroup("Safe Area")]
        [SerializeField] CanvasGroup noMoreMovesIndicator;
        public CrystalTimerVisualiser CrystalGameTimer => gameplayTimer;

        [BoxGroup("Buttons", "Buttons")]
        [SerializeField] Button pauseButton;
        [BoxGroup("Buttons")]
        [SerializeField] UIFadeAnimation pauseButtonFadeAnimation;
        [BoxGroup("Buttons")]
        [SerializeField] Button replayButton;
        [BoxGroup("Buttons")]
        [SerializeField] UIFadeAnimation replayButtonFadeAnimation;

        [BoxGroup("Power Ups", "Power Ups")]
        [SerializeField] CrystalPUUIController powerUpsUIController;
        public CrystalPUUIController PowerUpsUIController => powerUpsUIController;

        [BoxGroup("Popups", "Popups")]
        [SerializeField] CrystalReplayPopupBehavior replayPopupBehavior;
        [BoxGroup("Popups")]
        [SerializeField] CrystalStageCompletePopup stageCompletePopup;
        [BoxGroup("Popups")]
        [SerializeField] CrystalTimerStartPopup timerStartPopup;

        [BoxGroup("Message Box", "Message Box")]
        [SerializeField] CrystalMessageBox messageBox;
        public CrystalMessageBox MessageBox => messageBox;

        [BoxGroup("Dev")]
        [SerializeField] GameObject devOverlay;

        private AnimCase noMoreMovesCase;
        [BoxGroup("GAmeoverArrenge Maths Que Text", "GAmeoverArrenge Maths Que Text")]
        [SerializeField] private TextMeshProUGUI queText;

        private static CrystalUIGame instance;


        private Sequence iconSeq;
        [Group("Refs")]
        [SerializeField] Image clockImg;
        
        private void Awake()
        {
            instance = this;
        }

        public static TextMeshProUGUI QueText => instance.queText;
        public override void Init()
        {
            if (coinsPanel != null)
                coinsPanel.Init();

            if (pauseButton != null)
                pauseButton.onClick.AddListener(ShowPausePopUp);

            if (replayButton != null)
                replayButton.onClick.AddListener(ShowReplayPopup);

            if (pauseButtonFadeAnimation != null)
                pauseButtonFadeAnimation.Hide(immediately: true);

            if (replayButtonFadeAnimation != null)
                replayButtonFadeAnimation.Hide(immediately: true);

            if (messageBox != null)
                messageBox.Init();

            if (safeAreaRectTransform != null)
                SafeAreaHandler.RegisterRectTransform(safeAreaRectTransform);

            if (devOverlay != null)
                CrystalDevPanelEnabler.RegisterPanel(devOverlay);

            // Ensure timer popup starts hidden
            if (timerStartPopup != null)
            {
                timerStartPopup.Hide();
            }

            // Stage Panel disabled - prefabs not assigned
            // if (CrystalStagePanel != null)
            //     CrystalStagePanel.Init();
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            CrystalLevelController.LevelLoaded -= OnLevelLoaded;
            
            // Stage Panel disabled
            // if (CrystalStagePanel != null)
            //     CrystalStagePanel.Unload();
        }

        private void OnEnable()
        {
            // Subscribe to level loaded event to handle timer/popup after level is actually loaded
            CrystalLevelController.LevelLoaded += OnLevelLoaded;
        }

        private void OnDisable()
        {
            // Unsubscribe from level loaded event
            CrystalLevelController.LevelLoaded -= OnLevelLoaded;
        }

        private void OnLevelLoaded()
        {
            // Handle timer and popup AFTER level is actually loaded
            // This ensures DisplayLevelIndex is correct (especially when loading from grid)
            Debug.Log("[CrystalUIGame] OnLevelLoaded event received - handling timer/popup");
            HandleTimerAndPopupForCurrentLevel();
        }

        private void HandleTimerAndPopupForCurrentLevel()
        {
            // Get the current level (1-based) - this is now accurate because level is loaded
            int currentLevel = CrystalLevelController.DisplayedLevelIndex + 1;
            
            Debug.Log($"[CrystalUIGame] HandleTimerAndPopupForCurrentLevel - Level: {currentLevel}");

            // Always hide popup and timer first
            if (timerStartPopup != null)
                timerStartPopup.Hide(immediately: true);

            if (CrystalLevelController.GameTimer != null)
            {
                CrystalLevelController.GameTimer.Pause();
            }
            CrystalGameTimer.Hide();

            // Levels 1-10: No timer bar, no popup
            if (currentLevel <= 10)
            {
                Debug.Log("[CrystalUIGame] Level <= 10 : NO TIMER BAR, NO POPUP");
                // Timer and popup are already hidden above - nothing more to do
                return;
            }

            // Level 11: Show popup first, then show timer bar after popup is closed
            if (currentLevel == 11)
            {
                Debug.Log("[CrystalUIGame] Level 11 : Show popup, then show timer bar after popup closes");

                if (CrystalGameManager.Data.GameplayTimerEnabled &&
                    CrystalLevelController.GameTimer != null)
                {
                    // Timer is paused + hidden already; show popup first
                    if (timerStartPopup != null)
                    {
                        // Start coroutine with retry mechanism
                        StartCoroutine(ShowTimerPopupForLevel11WithRetry());
                    }
                    else
                    {
                        Debug.LogWarning("[CrystalUIGame] timerStartPopup not assigned - starting timer without popup.");
                        // Fallback: show timer directly if popup is not assigned
                        CrystalGameTimer.Show(CrystalLevelController.GameTimer);
                        CrystalLevelController.GameTimer.Start();
                    }
                }
                else
                {
                    Debug.LogWarning("[CrystalUIGame] GameplayTimerEnabled is false OR GameTimer is null on level 11");
                }
                return;
            }

            // Level 12+: Timer bar visible and running, no popup
            if (currentLevel >= 12)
            {
                Debug.Log("[CrystalUIGame] Level >= 12 : Show timer bar normally (no popup)");

                if (CrystalGameManager.Data.GameplayTimerEnabled &&
                    CrystalLevelController.GameTimer != null)
                {
                    // Show timer bar and start it immediately
                    CrystalGameTimer.Show(CrystalLevelController.GameTimer);
                    CrystalLevelController.GameTimer.Start();
                }
            }
        }

        #region Show/Hide
        public override void PlayShowAnimation()
        {
            Debug.Log("[CrystalUIGame] PlayShowAnimation called");

            try
            {
                pauseButton.gameObject.SetActive(true);
                levelNumberText.gameObject.SetActive(true);

                coinsPanel.Activate();
                pauseButtonFadeAnimation.Show();

                replayButton.gameObject.SetActive(true);
                replayButtonFadeAnimation.Show();
                replayButton.interactable = true;

                messageBox.Disable();

                CrystalUILevelNumberText.Show();

                // --- always hide popup + timer first (will be shown later in HandleTimerAndPopupForCurrentLevel) ---
                if (timerStartPopup != null)
                    timerStartPopup.Hide(immediately: true);

                if (CrystalLevelController.GameTimer != null)
                {
                    CrystalLevelController.GameTimer.Pause();
                }
                CrystalGameTimer.Hide();

                // NOTE: Timer and popup logic is now handled in HandleTimerAndPopupForCurrentLevel()
                // which is called from OnLevelLoaded() event AFTER the level is actually loaded.
                // This ensures DisplayLevelIndex is correct when checking the level.
                // 
                // For levels loaded from grid, PlayShowAnimation() is called BEFORE LoadLevel() sets DisplayLevelIndex,
                // so we defer the timer/popup logic until after the level is loaded.
                
                Debug.Log("[CrystalUIGame] PlayShowAnimation - Timer/popup will be handled after level loads via OnLevelLoaded event");
                
                ScreenManager.OnPageOpened(this);
                Debug.Log("[CrystalUIGame] PlayShowAnimation completed successfully");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[CrystalUIGame] Error in PlayShowAnimation: {e.Message}\n{e.StackTrace}");
            }
        }

        public override void PlayHideAnimation()
        {
            coinsPanel.Disable();
            pauseButtonFadeAnimation.Hide();
            replayButtonFadeAnimation.Hide(onCompleted: () => ScreenManager.OnPageClosed(this));

            messageBox.Disable();

            CrystalUILevelNumberText.Hide();

            if (CrystalGameManager.Data.GameplayTimerEnabled)
            {
                CrystalGameTimer.Hide();
            }

            // Always hide timer popup when leaving game screen
            if (timerStartPopup != null)
            {
                timerStartPopup.Hide(immediately: true);
            }

            if (noMoreMovesIndicator.gameObject.activeSelf) HideNoMoreMovesIndicator(true);
        }

        public void UpdateLevelNumber(int levelNumber)
        {
            levelNumberText.UpdateLevelNumber(levelNumber);
        }
        #endregion

        public void SpawnLevelStages(int stages)
        {
            // Stage Panel disabled
            // CrystalStagePanel.Spawn(stages);
        }

        public void SetActiveStage(int stageIndex)
        {
            // Stage Panel disabled
            // CrystalStagePanel.Activate(stageIndex);
        }

        public void GideLevelStages()
        {
            // Stage Panel disabled
            // CrystalStagePanel.Clear();
        }

        public void ShowPausePopUp()
        {
            ScreenManager.DisplayScreen<CrystalUIPause>();

            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif
        }

        public void ShowReplayPopup()
        {
            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif
            replayPopupBehavior.Show();
        }

        public void SetReplayButtonInteractable(bool interactable)
        {
            if (replayButton != null)
                replayButton.interactable = interactable;
        }
        public override void PlayShowAnimationMainReturn()
        {

        }
        public void ShowNoMoreMovesIndicator()
        {
            if (noMoreMovesIndicator.gameObject.activeSelf && noMoreMovesIndicator.alpha == 1) return;

            noMoreMovesCase.KillActive();

            noMoreMovesIndicator.gameObject.SetActive(true);
            noMoreMovesCase = noMoreMovesIndicator.DOFade(1, 0.3f);
        }

        public void HideNoMoreMovesIndicator(bool instantly = false)
        {
            if (!noMoreMovesIndicator.gameObject.activeSelf) return;

            noMoreMovesCase.KillActive();

            if (instantly)
            {
                noMoreMovesIndicator.gameObject.SetActive(false);
                noMoreMovesIndicator.alpha = 0;
            }
            else
            {
                noMoreMovesCase = noMoreMovesIndicator.DOFade(0, 0.3f).OnComplete(() =>
                {
                    noMoreMovesIndicator.gameObject.SetActive(false);
                });
            }
        }

        public void ShowStageComplete(GameCallback onMaxFade = null)
        {
            stageCompletePopup.Show(onMaxFade);

        }

        #region Tutorial
        public void ActivateTutorial()
        {
            pauseButton.gameObject.SetActive(false);
            levelNumberText.gameObject.SetActive(false);
            replayButton.gameObject.SetActive(false);

            powerUpsUIController.HidePanels();

            if (CrystalGameManager.Data.GameplayTimerEnabled)
            {
                CrystalGameTimer.Hide();
            }
        }
        #endregion

        #region Development

        public void ReloadDev()
        {
            CrystalGameManager.ReplayLevel();
        }

        public void HideDev()
        {
            devOverlay.SetActive(false);
        }

        public void OnLevelInputUpdatedDev(string newLevel)
        {
            int level = -1;

            if (int.TryParse(newLevel, out level))
            {
                CrystalLevelSave CrystalLevelSave = DataManager.GetSaveObject<CrystalLevelSave>("level");
                CrystalLevelSave.DisplayLevelIndex = Mathf.Clamp((level - 1), 0, int.MaxValue);
                if (CrystalLevelSave.DisplayLevelIndex >= CrystalLevelController.Database.AmountOfLevels)
                {
                    CrystalLevelSave.DisplayLevelIndex = CrystalLevelController.Database.AmountOfLevels - 1;
                }
                CrystalLevelSave.RealLevelIndex = CrystalLevelSave.DisplayLevelIndex;

                CrystalGameManager.ReplayLevel();
            }
        }

        public void PrevLevelDev()
        {
            CrystalLevelSave CrystalLevelSave = DataManager.GetSaveObject<CrystalLevelSave>("level");
            CrystalLevelSave.DisplayLevelIndex = Mathf.Clamp(CrystalLevelSave.DisplayLevelIndex - 1, 0, int.MaxValue);
            if (CrystalLevelSave.DisplayLevelIndex >= CrystalLevelController.Database.AmountOfLevels)
            {
                CrystalLevelSave.DisplayLevelIndex = CrystalLevelController.Database.AmountOfLevels - 1;
            }
            CrystalLevelSave.RealLevelIndex = CrystalLevelSave.DisplayLevelIndex;

            CrystalGameManager.ReplayLevel();
        }    

        public void NextLevelDev()
        {
            CrystalLevelSave CrystalLevelSave = DataManager.GetSaveObject<CrystalLevelSave>("level");

            CrystalLevelSave.DisplayLevelIndex = CrystalLevelSave.DisplayLevelIndex + 1;
            if (CrystalLevelSave.DisplayLevelIndex >= CrystalLevelController.Database.AmountOfLevels)
            {
                CrystalLevelSave.DisplayLevelIndex = CrystalLevelController.Database.AmountOfLevels - 1;
            }
            CrystalLevelSave.RealLevelIndex = CrystalLevelSave.DisplayLevelIndex;

            CrystalGameManager.ReplayLevel();
        }
        private IEnumerator ShowTimerPopupForLevel11WithRetry()
        {
            Debug.Log("[CrystalUIGame] ShowTimerPopupForLevel11WithRetry coroutine started");

            // Wait a few frames to ensure level is fully loaded and DisplayedLevelIndex is set
            yield return null;
            yield return null;
            
            // Retry mechanism: Wait up to 1 second for level to be properly set
            int maxRetries = 10;
            int retryCount = 0;
            int currentLevel = 0;
            
            while (retryCount < maxRetries)
            {
                currentLevel = CrystalLevelController.DisplayedLevelIndex + 1;
                Debug.Log($"[CrystalUIGame] Retry {retryCount + 1}/{maxRetries} - Current level: {currentLevel}");
                
                if (currentLevel == 11)
                {
                    Debug.Log("[CrystalUIGame] Level 11 confirmed! Proceeding with popup.");
                    break;
                }
                
                yield return new WaitForSeconds(0.1f);
                retryCount++;
            }

            // Final check - if still not level 11, abort
            if (currentLevel != 11)
            {
                Debug.LogWarning($"[CrystalUIGame] Level is {currentLevel}, not 11. Aborting popup. Showing timer directly as fallback.");
                // Fallback: show timer directly if we're past level 11
                if (currentLevel > 11 && CrystalLevelController.GameTimer != null)
                {
                    CrystalGameTimer.Show(CrystalLevelController.GameTimer);
                    CrystalLevelController.GameTimer.Start();
                }
                yield break;
            }
            
            yield return new WaitForSeconds(0.2f); // Additional small delay for UI stability

            if (timerStartPopup == null)
            {
                Debug.LogError("[CrystalUIGame] timerStartPopup is NULL in coroutine - showing timer directly");
                // Fallback: show timer directly if popup is missing
                if (CrystalLevelController.GameTimer != null)
                {
                    CrystalGameTimer.Show(CrystalLevelController.GameTimer);
                    CrystalLevelController.GameTimer.Start();
                }
                yield break;
            }

            if (CrystalLevelController.GameTimer == null)
            {
                Debug.LogError("[CrystalUIGame] GameTimer is NULL in coroutine");
                yield break;
            }

            // Ensure popup is not hidden before showing
            if (timerStartPopup != null)
            {
                timerStartPopup.Hide(immediately: true);
            }
            yield return new WaitForSeconds(0.1f);

            // Show popup - when user closes it, show timer bar and start timer
            Debug.Log("[CrystalUIGame] About to show timer popup for level 11");
            timerStartPopup.Show(() =>
            {
                Debug.Log("[CrystalUIGame] Timer popup closed -> showing timer bar and starting timer");
                // Show timer bar UI on top
                if (CrystalGameTimer != null && CrystalLevelController.GameTimer != null)
                {
                    CrystalGameTimer.Show(CrystalLevelController.GameTimer);
                    CrystalLevelController.GameTimer.Start();
                }
                else
                {
                    Debug.LogError("[CrystalUIGame] CrystalGameTimer or GameTimer is null when trying to show timer");
                }
            });

            // Verify popup actually showed (safety check)
            yield return new WaitForSeconds(0.2f);
            if (timerStartPopup != null && !timerStartPopup.IsOpened)
            {
                Debug.LogWarning("[CrystalUIGame] Popup did not show properly - showing timer directly as fallback");
                if (CrystalLevelController.GameTimer != null)
                {
                    CrystalGameTimer.Show(CrystalLevelController.GameTimer);
                    CrystalLevelController.GameTimer.Start();
                }
            }
        }
        #endregion
    }
}
