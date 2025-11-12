using UnityEngine;

namespace CrystalUnbolt
{
    /// <summary>
    /// Your Custom Lives/Hearts Configuration
    /// Optional - can disable if not needed!
    /// </summary>
    [CreateAssetMenu(fileName = "My Lives Config", menuName = "MyGame/Lives Configuration")]
    public class MyLivesConfig : ScriptableObject
    {
        [Header("=== Lives System ===")]
        [Tooltip("Enable lives/hearts system?")]
        public bool useLivesSystem = true;
        
        [Tooltip("Maximum lives player can have")]
        public int maxLives = 5;
        
        [Tooltip("Starting lives for new player")]
        public int startingLives = 5;
        
        [Header("=== Regeneration ===")]
        [Tooltip("Minutes to regenerate 1 life")]
        public int minutesPerLife = 15;
        
        [Header("=== Refill Options ===")]
        [Tooltip("Allow refilling lives with ad?")]
        public bool allowAdRefill = true;
        
        [Tooltip("Lives gained from watching ad")]
        public int livesFromAd = 1;
        
        [Tooltip("Allow buying lives with coins?")]
        public bool allowCoinRefill = false;
        public int coinCostPerLife = 50;
        
        [Header("=== Display ===")]
        public Sprite heartFullIcon;
        public Sprite heartEmptyIcon;
    }
}






