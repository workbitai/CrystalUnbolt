using System.Collections.Generic;
using UnityEngine;

namespace CrystalUnbolt
{
    [CreateAssetMenu(fileName = "Plank Skin Data", menuName = "Content/Skins/Plank Skin Data")]
    public class CrystalPlanksSkinData : ScriptableObject
    {
        [SerializeField] List<CrystalPlankData> planks = new List<CrystalPlankData>();
        public List<CrystalPlankData> Planks => planks;

        [Space]
        [SerializeField] GameObject plankHolePrefab;
        public GameObject PlankHolePrefab => plankHolePrefab;

        [SerializeField] GameObject baseHolePrefab;
        public GameObject BaseHolePrefab => baseHolePrefab;

        [SerializeField] GameObject screwPrefab;
        public GameObject ScrewPrefab => screwPrefab;

        [SerializeField] GameObject backPrefab;
        public GameObject BackPrefab => backPrefab;

        [SerializeField] List<Color> layerColors;

        public CrystalPlankData GetPlankData(CrystalPlankType CrystalPlankType)
        {
            for (int i = 0; i < planks.Count; i++)
            {
                CrystalPlankData data = planks[i];

                if (data.Type == CrystalPlankType) return data;
            }

            return null;
        }

        public Color GetLayerColor(int layerIndex)
        {
            return layerColors[layerIndex % layerColors.Count];
        }
    }
}
