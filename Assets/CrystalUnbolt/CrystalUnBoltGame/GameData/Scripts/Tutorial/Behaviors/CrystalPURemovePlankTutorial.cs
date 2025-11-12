using UnityEngine;

namespace CrystalUnbolt
{
    public class CrystalPURemovePlankTutorial : CrystalBaseTutorial
    {
        private readonly CrystalPUType POWER_UP_TYPE = CrystalPUType.DestroyPlank;

        [Space]
        [SerializeField] Color textHighlightColor = Color.red;

        [Space]
        [SerializeField] string firstMessage = "Use this booster to<br><color={0}>remove</color> a plank from the board.";
        [SerializeField] string secondMessage = "<color={0}>Tap</color> on plank to remove it!";

        public override bool IsActive => saveData.isActive;
        public override bool IsFinished => saveData.isFinished;
        public override int Progress => saveData.progress;

        private CrystalTutorialBaseSave saveData;

        private CrystalUIGame gameUI;

        private CrystalPUUIBehavior powerUpPanel;

        private bool isPUUsed;

        public override void Init()
        {
            if (isInitialised) return;

            isInitialised = true;

            // Load save file
            saveData = DataManager.GetSaveObject<CrystalTutorialBaseSave>(string.Format(CrystalITutorial.SAVE_IDENTIFIER, TutorialID.ToString()));

            gameUI = ScreenManager.GetPage<CrystalUIGame>();

            if (saveData.isFinished) return;

            CrystalPUController.Unlocked += OnPUUnlocked;
        }

        public override void Unload()
        {
            isInitialised = false;
        }

        private void OnPUUnlocked(CrystalPUType powerUpType)
        {
            if (powerUpType != POWER_UP_TYPE) return;

            StartTutorial();
        }

        private void OnLevelLeft()
        {
            if (saveData.isFinished) return;

            saveData.isFinished = true;

            CrystalLevelController.LevelLeft -= OnLevelLeft;
            powerUpPanel.Behavior.SelectStateChanged -= OnSelectStateChanged;
            CrystalPUController.Used -= OnPUUsed;

            CrystalTutorialCanvasController.ResetTutorialCanvas();
            CrystalTutorialCanvasController.ResetPointer();

            gameUI.CrystalGameTimer.Hide();
        }

        public override void StartTutorial()
        {
            isPUUsed = false;

            CrystalLevelController.LevelLeft += OnLevelLeft;

            // Pause and hide gameplay timer
            CrystalLevelController.GameTimer.Pause();
            gameUI.CrystalGameTimer.Hide();

            powerUpPanel = gameUI.PowerUpsUIController.GetPanel(POWER_UP_TYPE);

            if (powerUpPanel.Behavior.Settings.Save.Amount <= 0)
                powerUpPanel.Behavior.Settings.Save.Amount = 1;

            Debug.Log("Redraw");

            powerUpPanel.Redraw();
            powerUpPanel.Behavior.SelectStateChanged += OnSelectStateChanged;

            HighlightPU();
        }

        private void OnSelectStateChanged(bool value)
        {
            if (isPUUsed) return;

            if(value)
            {
                CrystalTutorialCanvasController.ResetTutorialCanvas();
                CrystalTutorialCanvasController.ResetPointer();

                gameUI.MessageBox.Activate(string.Format(secondMessage, textHighlightColor.ToHex()), new Vector3(0, 5.2f, 0));

                CrystalPUController.Used += OnPUUsed;
            }
            else
            {
                HighlightPU();

                CrystalPUController.Used -= OnPUUsed;
            }
        }

        private void OnPUUsed(CrystalPUType powerUpType)
        {
            if (powerUpType != POWER_UP_TYPE) return;

            isPUUsed = true;

            powerUpPanel.Behavior.SelectStateChanged -= OnSelectStateChanged;
            CrystalPUController.Used -= OnPUUsed;

            gameUI.MessageBox.Disable();

            CrystalTutorialCanvasController.ResetPointer();

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
            if (saveData.isFinished) return;

            saveData.isFinished = true;
        }
    }
}
