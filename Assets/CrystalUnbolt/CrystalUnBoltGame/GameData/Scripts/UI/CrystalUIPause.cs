using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalUnbolt
{
    public class CrystalUIPause : BaseScreen, IPopupWindow
    {
        [BoxGroup("References", "References")]
        [SerializeField] Image backgroundImage;
        [BoxGroup("References", "References")]
        [SerializeField] RectTransform panel;

        [BoxGroup("Buttons", "Buttons")]
        [SerializeField] Button closeButton;
        [BoxGroup("Buttons", "Buttons")]
        [SerializeField] Button continueButton;
        [BoxGroup("Buttons", "Buttons")]
        [SerializeField] Button quitButton;

        [BoxGroup("Popup", "Popup")]
        [SerializeField] CrystalUILevelQuitPopUp levelQuitPopUp;

      

        public bool IsOpened => isPageDisplayed;

        public override void Init()
        {
            continueButton.onClick.AddListener(OnContinueButtonClicked);
            quitButton.onClick.AddListener(OnQuitButtonClicked);
            closeButton.onClick.AddListener(OnContinueButtonClicked);
        }

        public override void PlayShowAnimation()
        {
            panel.anchoredPosition = Vector2.down * 2000;
            panel.DOAnchoredPosition(Vector2.zero, 0.3f).SetEasing(Ease.Type.SineOut);

            backgroundImage.SetAlpha(0);
            backgroundImage.DOFade(0.3f, 0.3f);

            if (CrystalGameManager.Data.GameplayTimerEnabled)
                CrystalLevelController.GameTimer.Pause();

            ScreenManager.OnPageOpened(this);
            ScreenManager.OnPopupWindowOpened(this);

            PopupHelper.ShowPopup(continueButton.transform);
            PopupHelper.ShowPopup(quitButton.transform);
        }

        public override void PlayHideAnimation()
        {
            panel.DOAnchoredPosition(Vector2.down * 2000, 0.3f).SetEasing(Ease.Type.SineIn);

            backgroundImage.DOFade(0, 0.3f);

            ScreenManager.OnPageClosed(this);
            ScreenManager.OnPopupWindowClosed(this);
        }

        private void OnEnable()
        {
            levelQuitPopUp.OnConfirmExitEvent += ExitPopUpConfirmExitButton;
            levelQuitPopUp.OnCancelExitEvent += ExitPopCloseButton;
        }

        private void OnDisable()
        {
            levelQuitPopUp.OnConfirmExitEvent -= ExitPopUpConfirmExitButton;
            levelQuitPopUp.OnCancelExitEvent += ExitPopCloseButton;
        }

        public void ExitPopCloseButton()
        {
            levelQuitPopUp.Hide();
        }

        public void ExitPopUpConfirmExitButton()
        {
            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);

            CrystalLivesSystem.UnlockLife(true);

            ScreenOverlay.Show(0.3f, () =>
            {
                levelQuitPopUp.Hide();

                ScreenManager.DisableScreen<CrystalUIPause>();
                ScreenManager.DisableScreen<CrystalUIGame>();

                ScreenManager.OnPopupWindowClosed(this);

                CrystalGameManager.ReturnToMenu();

                ScreenOverlay.Hide(0.3f);
            });
        }

        private void OnQuitButtonClicked()
        {
            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif
            if (CrystalLivesSystem.InfiniteMode)
            {
                ExitPopUpConfirmExitButton();
            }
            else
            {
                levelQuitPopUp.Show();
            }
        }

        private void OnContinueButtonClicked()
        {
            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif
            ScreenManager.CloseScreen<CrystalUIPause>(() =>
            {
                if (CrystalGameManager.Data.GameplayTimerEnabled)
                    CrystalLevelController.GameTimer.Resume();
            });
        }
        public override void PlayShowAnimationMainReturn()
        {

        }
    }
}
