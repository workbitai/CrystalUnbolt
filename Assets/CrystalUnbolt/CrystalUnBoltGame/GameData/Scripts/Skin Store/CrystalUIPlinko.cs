using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalUnbolt
{
    public class CrystalUIPlinko : BaseScreen
    {
        [Header("References")]
      
        [SerializeField] private Button  pauseButton;
        private SimpleBoolCallback panelClosed;
        [SerializeField] CrystalUILevelQuitPopUp levelQuitPopUp;

        public override void Init()
        {
            // Initialize the Plinko screen
            if (pauseButton != null)
                pauseButton.onClick.AddListener(ShowPausePopUp);
        }

        public override void PlayHideAnimation()
        {
            // Play hide animation
            DisableCanvas();
        }

        public override void PlayShowAnimation()
        {
            // Play show animation
            EnableCanvas();
        }

        public override void PlayShowAnimationMainReturn()
        {
            // Play show animation when returning to main
            EnableCanvas();
        }

        public void ShowPausePopUp()
        {
            ScreenManager.DisplayScreen<CrystalUIPause>();

            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif
        }
    }
}
