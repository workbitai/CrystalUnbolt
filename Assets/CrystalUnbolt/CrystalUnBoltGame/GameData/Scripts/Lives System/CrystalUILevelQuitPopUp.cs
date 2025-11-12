using UnityEngine;
using UnityEngine.UI;

namespace CrystalUnbolt
{
    public class CrystalUILevelQuitPopUp : MonoBehaviour, IPopupWindow
    {
        [SerializeField] Button closeSmallButton;
        [SerializeField] Button closeBigButton;
        [SerializeField] Button confirmButton;

        public GameCallback OnCancelExitEvent;
        public GameCallback OnConfirmExitEvent;

        [SerializeField] RectTransform quitHeartIcon;

        public bool IsOpened => gameObject.activeSelf;

        private void Awake()
        {
            closeSmallButton.onClick.AddListener(ExitPopCloseButton);
            closeBigButton.onClick.AddListener(ExitPopCloseButton);
            confirmButton.onClick.AddListener(ExitPopUpConfirmExitButton);
        }

        public void Show()
        {
            gameObject.SetActive(true);

            ScreenManager.OnPopupWindowOpened(this);
            PopupHelper.ShowPopup(quitHeartIcon);
            PopupHelper.ShowPopup(confirmButton.transform);
        }

        public void Hide()
        {
            gameObject.SetActive(false);

            ScreenManager.OnPopupWindowClosed(this);
        }

        public void ExitPopCloseButton()
        {
            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif
            OnCancelExitEvent?.Invoke();

            gameObject.SetActive(false);
        }

        public void ExitPopUpConfirmExitButton()
        {
            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif
            OnConfirmExitEvent?.Invoke();

            gameObject.SetActive(false);
        }
    }
}
