using UnityEngine;

namespace CrystalUnbolt
{
    /// <summary>
    /// Your Custom Currency Configuration
    /// Manage game coins/currency
    /// </summary>
    [CreateAssetMenu(fileName = "My Currency Config", menuName = "MyGame/Currency Configuration")]
    public class MyCurrencyConfig : ScriptableObject
    {
        [Header("=== Currency Settings ===")]
        [Tooltip("Starting coins for new players")]
        public int startingCoins = 100;
        
        [Tooltip("Coins per level complete")]
        public int coinsPerLevel = 50;
        
        [Tooltip("Bonus coins for perfect level (no mistakes)")]
        public int perfectBonus = 20;
        
        [Tooltip("Coins from watching rewarded ad")]
        public int coinsFromAd = 100;
        
        [Header("=== Display ===")]
        public Sprite coinIcon;
        public Color coinColor = Color.yellow;
        
        [Header("=== Rewards ===")]
        [Tooltip("Daily login bonus")]
        public int dailyBonusCoins = 50;
        
        [Tooltip("Spin wheel rewards")]
        public int[] spinRewards = new int[] { 50, 100, 150, 200, 500 };
    }
}






