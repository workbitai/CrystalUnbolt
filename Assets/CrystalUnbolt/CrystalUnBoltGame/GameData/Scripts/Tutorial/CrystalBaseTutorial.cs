using UnityEngine;

namespace CrystalUnbolt
{
    public abstract class CrystalBaseTutorial : MonoBehaviour, CrystalITutorial
    {
        [SerializeField] 
        protected CrystalTutorialID tutorialID;
        public CrystalTutorialID TutorialID => tutorialID;

        [SerializeField] bool autoInitialise;

        public abstract bool IsActive { get; }
        public abstract bool IsFinished { get; }
        public abstract int Progress { get; }

        protected bool isInitialised;
        public bool IsInitialised => isInitialised;

        private void OnEnable()
        {
            CrystalTutorialController.RegisterTutorial(this);

            if (autoInitialise)
                Init();
        }

        private void OnDisable()
        {
            CrystalTutorialController.RemoveTutorial(this);
        }

        public abstract void Init();

        public abstract void StartTutorial();
        public abstract void FinishTutorial();

        public abstract void Unload();
    }
}