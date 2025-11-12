#pragma warning disable 0649 

using UnityEngine;

namespace CrystalUnbolt
{
    public class CrystalSettingsLinkButton : CrystalSettingsButtonBase
    {
        [SerializeField] string url;

        public override void Init()
        {

        }

        public override void OnClick()
        {
            Application.OpenURL(url);

            // Play button sound
            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif
        }
    }
}