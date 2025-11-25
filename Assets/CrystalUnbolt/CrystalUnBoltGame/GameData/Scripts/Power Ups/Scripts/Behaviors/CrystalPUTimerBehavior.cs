using UnityEngine;

namespace CrystalUnbolt
{
    public class CrystalPUTimerBehavior : CrystalPUBehavior
    {
        private CrystalPUTimer timer;

        private CrystalPUTimerSettings timerSettings;

        public override void Init()
        {
            timerSettings = (CrystalPUTimerSettings)settings;

            timer = null;
        }

        public override bool IsActive()
        {
            return CrystalLevelController.ShouldRunTimerForCurrentLevel();
        }

        public override bool Activate()
        {
            IsBusy = true;

            if (CrystalLevelController.ShouldRunTimerForCurrentLevel() && CrystalLevelController.GameTimer != null)
            {
                CrystalLevelController.GameTimer.Pause();
            }

            timer = new CrystalPUTimer(timerSettings.TimeFreezeDuration, () =>
            {
                timer = null;
                IsBusy = false;

                if (CrystalLevelController.ShouldRunTimerForCurrentLevel() && CrystalLevelController.GameTimer != null)
                {
                    CrystalLevelController.GameTimer.Resume();
                }
            });

            return true;
        }

        public override void ResetBehavior()
        {
            if(timer != null)
            {
                timer.Disable();
                timer = null;
            }

            IsBusy = false;
        }

        public override CrystalPUTimer GetTimer()
        {
            return timer;
        }

        public override bool ApplyToElement(IClickableObject clickableObject, Vector3 clickPosition) => false;
        public override bool IsSelectable() => false;
    }
}
