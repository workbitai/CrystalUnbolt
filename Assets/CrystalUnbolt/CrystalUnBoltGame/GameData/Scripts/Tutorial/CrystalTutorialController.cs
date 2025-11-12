using System.Collections.Generic;
using UnityEngine;

namespace CrystalUnbolt
{
    public class CrystalTutorialController : MonoBehaviour
    {
        private static CrystalTutorialController tutorialController;
        private static List<CrystalITutorial> registeredTutorials = new List<CrystalITutorial>();

        [SerializeField] CrystalTutorialCanvasController tutorialCanvasController;

        private static bool isTutorialSkipped;

        public void Init()
        {
            tutorialController = this;

            isTutorialSkipped = CrystalTutorialHelper.IsTutorialSkipped();

            tutorialCanvasController.Init();
        }

        private void OnDestroy()
        {
            if(!registeredTutorials.IsNullOrEmpty())
            {
                foreach(CrystalITutorial tutorial in registeredTutorials)
                {
                    tutorial.Unload();
                }

                registeredTutorials.Clear();
            }
        }

        public static CrystalITutorial GetTutorial(CrystalTutorialID CrystalTutorialID)
        {
            for(int i = 0; i < registeredTutorials.Count; i++)
            {
                if (registeredTutorials[i].TutorialID == CrystalTutorialID)
                {
                    if (!registeredTutorials[i].IsInitialised)
                        registeredTutorials[i].Init();

                    if (isTutorialSkipped)
                        registeredTutorials[i].FinishTutorial();

                    return registeredTutorials[i];
                }
            }

            return null;
        }

        public static void ActivateTutorial(CrystalITutorial tutorial)
        {
            if (!tutorial.IsInitialised)
                tutorial.Init();

            if (isTutorialSkipped)
                tutorial.FinishTutorial();
        }

        public static void RegisterTutorial(CrystalITutorial tutorial)
        {
            if (registeredTutorials.FindIndex(x => x == tutorial) != -1)
                return;

            registeredTutorials.Add(tutorial);
        }

        public static void RemoveTutorial(CrystalITutorial tutorial)
        {
            int tutorialIndex = registeredTutorials.FindIndex(x => x == tutorial);
            if (tutorialIndex != -1)
            {
                registeredTutorials.RemoveAt(tutorialIndex);
            }
        }
    }
}