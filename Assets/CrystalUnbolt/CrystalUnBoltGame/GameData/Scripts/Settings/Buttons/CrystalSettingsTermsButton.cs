using UnityEngine;
using UnityEngine.EventSystems;

namespace CrystalUnbolt
{
    public class CrystalSettingsTermsButton : CrystalSettingsButtonBase
    {
        private string url;

        public override void Init()
        {
            // Monetization removed - set URL directly or disable
            url = "https://yourtermsofuse.com"; // Set your terms URL here!
            gameObject.SetActive(true);
        }

        public override void OnClick()
        {
            if (string.IsNullOrEmpty(url)) return;

            Application.OpenURL(url);

            // Play button sound
            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif
        }

        public override void Select()
        {
            IsSelected = true;

            EventSystem.current.SetSelectedGameObject(null); //clear any previous selection (best practice)
            EventSystem.current.SetSelectedGameObject(Button.gameObject, new BaseEventData(EventSystem.current));
        }

        public override void Deselect()
        {
            IsSelected = false;

            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}