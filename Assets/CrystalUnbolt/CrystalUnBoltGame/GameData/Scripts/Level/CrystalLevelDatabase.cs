#pragma warning disable 0649

using System.Collections.Generic;
using UnityEngine;

namespace CrystalUnbolt
{
    [CreateAssetMenu(menuName = "Content/Level Database/Level Database", fileName = "Level Database")]
    public class CrystalLevelDatabase : ScriptableObject
    {
        [SerializeField] CrystalLevelData[] levels;
        public CrystalLevelData[] Levels => levels;

        public int AmountOfLevels => levels.Length;

        public int GetRandomLevelIndex(int displayLevelNumber, int lastPlayedLevelNumber, bool replayingLevel)
        {
            if (levels.IsInRange(displayLevelNumber))
            {
                return displayLevelNumber;
            }

            if (replayingLevel)
            {
                return lastPlayedLevelNumber;
            }

            int randomLevelIndex;

            do
            {
                randomLevelIndex = Random.Range(0, levels.Length);
            }
            while (!levels[randomLevelIndex].UseInRandomizer && randomLevelIndex != lastPlayedLevelNumber);

            return randomLevelIndex;
        }

        public CrystalLevelData GetLevel(int i)
        {
            if (i < AmountOfLevels && i >= 0)
                return levels[i];

            return null;
        }
    }
}
