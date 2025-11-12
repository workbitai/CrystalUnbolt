using UnityEngine;
using UnityEngine.UI;

namespace CrystalUnbolt
{
    [RequireComponent(typeof(Button))]
    public class CrystalSettingsPanelButton : MonoBehaviour
    {
        public Button Button { get; private set; }

        private void Awake()
        {
            Button = GetComponent<Button>();
            Button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            ScreenManager.DisplayScreen<CrystalUISettings>();

            // Play button sound
            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif
        }
    }
}
