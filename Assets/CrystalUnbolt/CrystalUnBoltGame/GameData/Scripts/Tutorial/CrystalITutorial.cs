namespace CrystalUnbolt
{
    public interface CrystalITutorial
    {
        public const string SAVE_IDENTIFIER = "TUTORIAL:{0}";

        public CrystalTutorialID TutorialID { get; }
        
        public bool IsActive { get; }
        public bool IsFinished { get; }

        public bool IsInitialised { get; }

        public int Progress { get; }

        public void Init();
        public void StartTutorial();
        public void FinishTutorial();
        public void Unload();
    }
}