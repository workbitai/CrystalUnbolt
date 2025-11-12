using UnityEngine;

namespace CrystalUnbolt
{
    /// <summary>
    /// Adapter: Connects YOUR ads system to existing game code
    /// No need to change 50+ files! Just redirect calls!
    /// </summary>
    public static class MyAdsAdapter
    {
        private static MyAdsManager Manager => MyAdsManager.Instance;
        
        public static void Initialize()
        {
            if (Manager != null)
            {
                Manager.InitializeAds();
            }
            else
            {
                Debug.LogWarning("[MyAds] Manager not found! Create MyAdsManager in scene.");
            }
        }
        
        // Banner Methods
        public static void EnableBanner()
        {
            Manager?.ShowBanner();
        }
        
        public static void ShowBanner()
        {
            Manager?.ShowBanner();
        }
        
        public static void HideBanner()
        {
            Manager?.HideBanner();
        }
        
        public static void DestroyBanner()
        {
            Manager?.HideBanner();
        }
        
        public static void DisableBanner()
        {
            Manager?.HideBanner();
        }
        
        // Interstitial Methods
        public static void ShowInterstitial(System.Action onComplete)
        {
            Manager?.ShowInterstitial(onComplete);
        }
        
        // Rewarded Methods
        public static void ShowRewardBasedVideo(System.Action<bool> onComplete)
        {
            Manager?.ShowRewarded(onComplete);
        }
        
        // Dummy method for compatibility
        public static bool IsForcedAdEnabled()
        {
            return true;
        }
        
        public static void DisableForcedAd()
        {
            // Compatibility method - does nothing
            Debug.Log("[MyAds] DisableForcedAd called (no-op)");
        }
        
        // Settings access - Old system removed!
        // public static AdsSettings Settings
        // {
        //     get
        //     {
        //         return Monetization.AdsSettings;
        //     }
        // }
    }
}

