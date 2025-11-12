using System;

namespace CrystalUnbolt
{
    [System.Serializable]
    public class CrystalLivesSave : ISaveObject
    {
        public int LivesCount = -1;

        public bool LifeLocked = false;
        public long NewLifeDateBinary;

        public bool InfiniteLives = false;
        public long InfiniteLivesDateBinary;

        [NonSerialized] CrystalLivesStatus status;

        public void Init(CrystalLivesStatus status)
        {
            this.status = status;
        }

        public void Flush()
        {
            if (status == null) return;

            LivesCount = status.LivesCount;

            InfiniteLives = status.InfiniteMode;
            InfiniteLivesDateBinary = status.InfiniteModeDate.ToBinary();

            if(status.NewLifeTimerEnabled)
            {
                NewLifeDateBinary = status.NewLifeDate.ToBinary();
            }
            else
            {
                NewLifeDateBinary = (DateTime.Now + CrystalLivesSystem.OneLifeSpan).ToBinary();
            }
        }
    }
}