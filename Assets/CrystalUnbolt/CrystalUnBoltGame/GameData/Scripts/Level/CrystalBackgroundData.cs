using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrystalUnbolt
{
    [System.Serializable]
    public class CrystalBackgroundData
    {
        [SerializeField] GameObject backgroundPrefab;
        public GameObject BackgroundPrefab => backgroundPrefab;

        [SerializeField] int availableFromLevel;
        public int AvailableFromLevel => availableFromLevel;

        [SerializeField] int collectionId;
        public int CollectionId => collectionId;
    }
}