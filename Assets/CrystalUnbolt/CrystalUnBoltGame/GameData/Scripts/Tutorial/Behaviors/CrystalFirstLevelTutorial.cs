using System.Collections;
using UnityEngine;

namespace CrystalUnbolt
{
    public class CrystalFirstLevelTutorial : CrystalBaseTutorial
    {
        [Space]
        [SerializeField] Color textHighlightColor = Color.red;

        [Space]
        [SerializeField] string firstMessage = "<color={0}>Tap</color> to extract crystal!";
        [SerializeField] string secondMessage = "<color={0}>Tap</color> to place screw!";

        public override bool IsActive => saveData.isActive;
        public override bool IsFinished => saveData.isFinished;
        public override int Progress => saveData.progress;

        private CrystalTutorialBaseSave saveData;

        private CrystalUIGame gameUI;

        private CrystalScrewController screw;
        private CrystalBaseHole bottomHole;
        private CrystalBaseHole topHole;

        public override void Init()
        {
            if (isInitialised) return;

            isInitialised = true;

            saveData = DataManager.GetSaveObject<CrystalTutorialBaseSave>(string.Format(CrystalITutorial.SAVE_IDENTIFIER, TutorialID.ToString()));

            gameUI = ScreenManager.GetPage<CrystalUIGame>();
        }

        public override void Unload()
        {
            isInitialised = false;
        }

        public override void FinishTutorial()
        {
            if (saveData.isFinished) return;

            saveData.isFinished = true;
        }

        public override void StartTutorial()
        {
            CrystalLevelController.LevelLoaded += OnLevelLoaded;
        }

        private void OnLevelLoaded()
        {
            if (CrystalLevelController.DisplayedLevelIndex != 0)
            {
                CrystalLevelController.LevelLoaded -= OnLevelLoaded;
                return;
            }

            CrystalLevelController.LevelLoaded -= OnLevelLoaded;

            screw = CrystalLevelController.StageLoader.Screws[0];
            topHole = CrystalLevelController.StageLoader.BaseHoles[0];
            bottomHole = CrystalLevelController.StageLoader.BaseHoles[1];

            CrystalLevelController.GameTimer.Pause();

            gameUI.ActivateTutorial();

            HighlightFirstHole();

            bottomHole.StateChanged += OnScrewRemoved;

            screw.Selected += OnScrewSelected;
            screw.Deselected += OnScrewDeselected;
        }

        private void HighlightFirstHole()
        {
            gameUI.MessageBox.Activate(string.Format(firstMessage, textHighlightColor.ToHex()), bottomHole.transform.position + new Vector3(0, -2.8f, 0));
            CrystalTutorialCanvasController.ActivatePointer(bottomHole.transform.position, CrystalTutorialCanvasController.POINTER_DEFAULT);
        }

        private void HighlightSecondsHole()
        {
            gameUI.MessageBox.Activate(string.Format(secondMessage, textHighlightColor.ToHex()), topHole.transform.position + new Vector3(0, 2.4f, 0));
            CrystalTutorialCanvasController.ActivatePointer(topHole.transform.position, CrystalTutorialCanvasController.POINTER_DEFAULT);
        }

        private void OnScrewDeselected()
        {
            if (IsFinished) return;

            HighlightFirstHole();
        }

        private void OnScrewSelected()
        {
            HighlightSecondsHole();
        }

        private bool firstStepCompleted = false; 

        private void OnScrewRemoved(bool value)
        {
            if (!value)
            {
                gameUI.MessageBox.Disable();
                CrystalTutorialCanvasController.ResetPointer();

                bottomHole.StateChanged -= OnScrewRemoved;
                screw.Selected -= OnScrewSelected;
                screw.Deselected -= OnScrewDeselected;

                if (!firstStepCompleted)
                {
                    firstStepCompleted = true;
                    StartCoroutine(ShowSecondStepWithDelay());
                }
                else
                {
                    SoundManager.PlaySound(SoundManager.AudioClips.tutorialComplete);
                    CrystalParticlesController.PlayParticle("Tutorial Complete");

                    FinishTutorial();
                }
            }
        }

        private IEnumerator ShowSecondStepWithDelay()
        {
            yield return new WaitForSeconds(4.5f);

            ShowCustomStep("<color={0}>Pick</color> the correct screw!", new Vector3(0, 3f, 0), topHole.transform);

            screw.Selected += OnSecondScrewSelected;

            topHole.StateChanged += OnSecondStepRemoved;
        }


        private void OnSecondScrewSelected()
        {
            gameUI.MessageBox.Disable();
            CrystalTutorialCanvasController.ResetPointer();

            ShowCustomStep("<color={0}>Place</color> it in the answer hole!", new Vector3(0, -2.8f, 0), bottomHole.transform);
        }

        private void OnSecondStepRemoved(bool value)
        {
            if (!value)
            {
                gameUI.MessageBox.Disable();
                CrystalTutorialCanvasController.ResetPointer();

                topHole.StateChanged -= OnSecondStepRemoved;

                SoundManager.PlaySound(SoundManager.AudioClips.tutorialComplete);
                CrystalParticlesController.PlayParticle("Tutorial Complete");

                FinishTutorial();
            }
        }
        private void ShowCustomStep(string customMessage, Vector3 offset, Transform target)
        {
            string formattedMsg = string.Format(customMessage, textHighlightColor.ToHex());
            gameUI.MessageBox.Activate(formattedMsg, target.position + offset);
            CrystalTutorialCanvasController.ActivatePointer(target.position, CrystalTutorialCanvasController.POINTER_DEFAULT);
        }


    }
}
