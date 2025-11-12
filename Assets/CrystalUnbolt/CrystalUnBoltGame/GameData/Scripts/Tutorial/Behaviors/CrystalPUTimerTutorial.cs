using UnityEngine;

namespace CrystalUnbolt
{
    public class CrystalPUTimerTutorial : CrystalBaseTutorial
    {
        private readonly CrystalPUType POWER_UP_TYPE = CrystalPUType.Timer;

        [Space]
        [SerializeField] Color textHighlightColor = Color.red;

        [Space]
        [SerializeField] string firstMessage = "Use this booster to<br>freeze the timer.";

        public override bool IsActive => saveData.isActive;
        public override bool IsFinished => saveData.isFinished;
        public override int Progress => saveData.progress;

        private CrystalTutorialBaseSave saveData;

        private CrystalUIGame gameUI;

        private CrystalPUUIBehavior powerUpPanel;

        public override void Init()
        {
            if (isInitialised)
                return;

            isInitialised = true;

            // Load save file
            saveData = DataManager.GetSaveObject<CrystalTutorialBaseSave>(string.Format(CrystalITutorial.SAVE_IDENTIFIER, TutorialID.ToString()));

            gameUI = ScreenManager.GetPage<CrystalUIGame>();

            if (saveData.isFinished)
                return;

            CrystalPUController.Unlocked += OnPUUnlocked;
        }

        public override void Unload()
        {
            isInitialised = false;
        }

        private void OnPUUnlocked(CrystalPUType powerUpType)
        {
            if (powerUpType != POWER_UP_TYPE)
                return;

            StartTutorial();
        }

        private void OnLevelLeft()
        {
            if (saveData.isFinished)
                return;

            saveData.isFinished = true;

            CrystalLevelController.LevelLeft -= OnLevelLeft;
            CrystalPUController.Used -= OnPUUsed;

            CrystalTutorialCanvasController.ResetTutorialCanvas();
            CrystalTutorialCanvasController.ResetPointer();

            gameUI.CrystalGameTimer.Hide();
        }

        public override void StartTutorial()
        {
            powerUpPanel = gameUI.PowerUpsUIController.GetPanel(POWER_UP_TYPE);

            if (powerUpPanel.Behavior.Settings.Save.Amount <= 0)
                powerUpPanel.Behavior.Settings.Save.Amount = 1;

            Debug.Log("Redraw");

            powerUpPanel.Redraw();

            CrystalLevelController.LevelLeft += OnLevelLeft;
            CrystalPUController.Used += OnPUUsed;

            HighlightPU();
        }

        private void OnPUUsed(CrystalPUType powerUpType)
        {
            if (powerUpType != POWER_UP_TYPE)
                return;

            CrystalPUController.Used -= OnPUUsed;

            gameUI.MessageBox.Disable();

            CrystalTutorialCanvasController.ResetPointer();
            CrystalTutorialCanvasController.ResetTutorialCanvas();

            SoundManager.PlaySound(SoundManager.AudioClips.tutorialComplete);

            CrystalParticlesController.PlayParticle("Tutorial Complete");

            FinishTutorial();
        }

        private void HighlightPU()
        {
            gameUI.MessageBox.Activate(string.Format(firstMessage, textHighlightColor.ToHex()), powerUpPanel.transform.position + new Vector3(0, 6f, 0));
            gameUI.MessageBox.ActivateTutorial();

            CrystalTutorialCanvasController.ActivateTutorialCanvas((RectTransform)powerUpPanel.transform, true, true);
            CrystalTutorialCanvasController.ActivatePointer(powerUpPanel.transform.position + new Vector3(0, 2.8f, 0), CrystalTutorialCanvasController.POINTER_SHOW_PU);
        }

        public override void FinishTutorial()
        {
            if (saveData.isFinished)
                return;

            saveData.isFinished = true;
        }
    }
}
