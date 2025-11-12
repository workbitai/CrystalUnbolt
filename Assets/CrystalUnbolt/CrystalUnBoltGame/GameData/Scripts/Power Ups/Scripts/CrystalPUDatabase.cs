using UnityEngine;

namespace CrystalUnbolt
{
    [CreateAssetMenu(fileName = "Power Ups Database", menuName = "Content/Power Ups/Database")]
    public class CrystalPUDatabase : ScriptableObject
    {
        [SerializeField] CrystalPUSettings[] powerUps;
        public CrystalPUSettings[] PowerUps => powerUps;
    }
}
