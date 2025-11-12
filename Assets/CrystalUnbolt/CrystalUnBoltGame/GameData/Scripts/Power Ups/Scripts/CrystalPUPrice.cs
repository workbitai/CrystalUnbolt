using UnityEngine;

namespace CrystalUnbolt
{
    [System.Serializable]
    public class CrystalPUPrice
    {
        [SerializeField] CrystalPUType powerUpType;
        public CrystalPUType PowerUpType => powerUpType;

        [SerializeField] int amount;
        public int Amount => amount;
    }
}
