using UnityEngine.EventSystems;

namespace CrystalUnbolt
{
    public class CrystalSettingsRestoreButton : CrystalSettingsButtonBase
    {
        public override void Init()
        {
            // Monetization removed - Restore button only for iOS (Apple requirement)
            #if UNITY_IOS
                gameObject.SetActive(true);
            #else
                gameObject.SetActive(false);
            #endif
        }

        public override void OnClick()
        {
#if MODULE_MONETIZATION
            // IAPManager. // IAP Disabled!RestorePurchases();
#endif

            // Play button sound
            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif
        }

        public override void Select()
        {
            IsSelected = true;

            Button.Select();

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