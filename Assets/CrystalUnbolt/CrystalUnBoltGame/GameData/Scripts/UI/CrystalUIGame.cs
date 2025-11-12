using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
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

                if (CrystalGameManager.Data.GameplayTimerEnabled)
                {
                    CrystalGameTimer.Show(CrystalLevelController.GameTimer);
                }

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
                noMoreMovesCase = noMoreMovesIndicator.DOFade(0, 0.3f).OnComplete(() => {
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
        #endregion
    }
}