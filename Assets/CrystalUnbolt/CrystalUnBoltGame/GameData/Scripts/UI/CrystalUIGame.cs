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
        private void OnEnable()
        {
        }

        private void OnDisable()
        {
        }
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
            // Stage Panel disabled
            // if (CrystalStagePanel != null)
            //     CrystalStagePanel.Unload();
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

                // --- always hide popup + timer first ---
                if (timerStartPopup != null)
                    timerStartPopup.Hide(immediately: true);

                if (CrystalLevelController.GameTimer != null)
                {
                    CrystalLevelController.GameTimer.Pause();
                }
                CrystalGameTimer.Hide();

                // ----- GET CURRENT LEVEL NUMBER (1-based) -----
                // Use the same save object that dev buttons use,
                // so level number is always correct even when you replay.
                var levelSave = DataManager.GetSaveObject<CrystalLevelSave>("level");
                int currentLevel = (levelSave != null ? levelSave.DisplayLevelIndex : 0) + 1;

                Debug.Log($"[CrystalUIGame] ===== LEVEL CHECK START =====  currentLevel = {currentLevel}");

                // 1–10  =>  no timer, no popup  (always)
                if (currentLevel <= 10)
                {
                    Debug.Log("[CrystalUIGame] Level <= 10 : NO TIMER, NO POPUP");
                    // everything already hidden above
                }
                // 11   =>  show popup, then start timer
                else if (currentLevel == 11)
                {
                    Debug.Log("[CrystalUIGame] Level 11 : show popup then start timer");

                    if (CrystalGameManager.Data.GameplayTimerEnabled &&
                        CrystalLevelController.GameTimer != null)
                    {
                        // timer is paused + hidden already; just show popup
                        if (timerStartPopup != null)
                        {
                            StartCoroutine(ShowTimerPopupForLevel11());
                        }
                        else
                        {
                            Debug.LogWarning("[CrystalUIGame] timerStartPopup not assigned – starting timer without popup.");
                            CrystalGameTimer.Show(CrystalLevelController.GameTimer);
                            CrystalLevelController.GameTimer.Start();
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[CrystalUIGame] GameplayTimerEnabled is false OR GameTimer is null on level 11");
                    }
                }
                // 12+  =>  timer visible and running, no popup
                else
                {
                    Debug.Log("[CrystalUIGame] Level >= 12 : show timer normally");

                    if (CrystalGameManager.Data.GameplayTimerEnabled &&
                        CrystalLevelController.GameTimer != null)
                    {
                        CrystalGameTimer.Show(CrystalLevelController.GameTimer);
                        CrystalLevelController.GameTimer.Start();
                    }
                }

                Debug.Log("[CrystalUIGame] ===== LEVEL CHECK END =====");
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
        private IEnumerator ShowTimerPopupForLevel11()
        {
            Debug.Log("[CrystalUIGame] ShowTimerPopupForLevel11 coroutine started");

            // small delay so UI is ready
            yield return null;
            yield return new WaitForSeconds(0.3f);

            if (timerStartPopup == null)
            {
                Debug.LogError("[CrystalUIGame] timerStartPopup is NULL in coroutine");
                yield break;
            }

            if (CrystalLevelController.GameTimer == null)
            {
                Debug.LogError("[CrystalUIGame] GameTimer is NULL in coroutine");
                yield break;
            }

            // show popup and then start timer
            timerStartPopup.Show(() =>
            {
                Debug.Log("[CrystalUIGame] Timer popup closed -> starting timer");
                CrystalGameTimer.Show(CrystalLevelController.GameTimer);
                CrystalLevelController.GameTimer.Start();
            });
        }
        #endregion
    }
}