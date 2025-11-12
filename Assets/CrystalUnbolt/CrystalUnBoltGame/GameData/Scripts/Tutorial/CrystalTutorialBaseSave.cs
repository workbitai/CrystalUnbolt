using UnityEngine;
using UnityEngine.UI;

namespace CrystalUnbolt
{
    [System.Serializable]
    public class CrystalTutorialBaseSave : ISaveObject
    {
        public bool isActive;
        public bool isFinished;

        public int progress;

        public void Flush()
        {

        }
    }
}