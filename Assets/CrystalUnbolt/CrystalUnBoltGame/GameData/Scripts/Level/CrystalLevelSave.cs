namespace CrystalUnbolt
{
    [System.Serializable]
    public class CrystalLevelSave : ISaveObject
    {
        public int MaxReachedLevelIndex = 0;

        public int RealLevelIndex = 0;
        public int DisplayLevelIndex = 0;
        public bool IsPlayingRandomLevel = false;

        public int LastPlayerLevelIndex = -1;
        
        public void Flush()
        {

        }
    }
}
