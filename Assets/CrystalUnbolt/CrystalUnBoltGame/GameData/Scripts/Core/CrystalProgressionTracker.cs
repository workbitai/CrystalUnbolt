using UnityEngine;

namespace CrystalUnbolt
{
    /// <summary>
    /// Custom progression tracking - Unique to CrystalUnbolt
    /// </summary>
    public static class CrystalProgressionTracker
    {
        private const string TIER_KEY = "Crystal_Tier";
        private const string XP_KEY = "Crystal_XP";
        
        public static int CurrentTier
        {
            get => PlayerPrefs.GetInt(TIER_KEY, 1);
            private set => PlayerPrefs.SetInt(TIER_KEY, value);
        }
        
        public static int CurrentXP
        {
            get => PlayerPrefs.GetInt(XP_KEY, 0);
            private set => PlayerPrefs.SetInt(XP_KEY, value);
        }
        
        public static void AddXP(int amount)
        {
            CurrentXP += amount;
            CheckTierUp();
            PlayerPrefs.Save();
        }
        
        private static void CheckTierUp()
        {
            int xpNeeded = GetXPForNextTier();
            while (CurrentXP >= xpNeeded)
            {
                CurrentXP -= xpNeeded;
                CurrentTier++;
                OnTierUp();
                xpNeeded = GetXPForNextTier();
            }
        }
        
        private static int GetXPForNextTier()
        {
            // Unique formula: exponential growth
            return 100 + (CurrentTier * 50);
        }
        
        private static void OnTierUp()
        {
            // Unique reward pattern
            int coinReward = CurrentTier * 75;
            if (MyCurrencyManager.Instance != null)
            {
                MyCurrencyManager.Instance.AddCoins(coinReward);
            }
        }
        
        public static float GetProgressPercent()
        {
            return (float)CurrentXP / GetXPForNextTier();
        }
    }
}


