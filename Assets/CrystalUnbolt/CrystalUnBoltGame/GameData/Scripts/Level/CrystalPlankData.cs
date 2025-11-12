#pragma warning disable 0414

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrystalUnbolt
{
    [System.Serializable]
    public class CrystalPlankData 
    {
        [SerializeField] CrystalPlankType type;
        public CrystalPlankType Type => type;

        [SerializeField] GameObject prefab;
        public GameObject Prefab => prefab;

        [SerializeField] bool useInQuickMode;
        public bool UseInQuickMode => useInQuickMode;

        [SerializeField] float quickModeSize;
        public float QuickModeSize => quickModeSize;

        public CrystalPlankData()
        {
        }

        public CrystalPlankData(CrystalPlankType type, GameObject prefab, bool useInQuickMode, float quickModeSize = 0)
        {
            this.type = type;
            this.prefab = prefab;
            this.useInQuickMode = useInQuickMode;
            this.quickModeSize = quickModeSize;
        }
    }
}
