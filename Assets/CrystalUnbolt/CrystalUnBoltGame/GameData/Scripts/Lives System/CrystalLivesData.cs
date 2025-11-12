using UnityEngine;

namespace CrystalUnbolt
{
    [CreateAssetMenu(fileName = "Lives Data", menuName = "Data/Lives")]
    public class CrystalLivesData : ScriptableObject
    {
        [SerializeField] int maxLivesCount = 5;
        public int MaxLivesCount => maxLivesCount;

        [Tooltip("In seconds")]
        [SerializeField] int oneLifeRestorationDuration = 1200;
        public int OneLifeRestorationDuration => oneLifeRestorationDuration;
    }
}