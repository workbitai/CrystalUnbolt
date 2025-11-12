using UnityEngine;

namespace CrystalUnbolt
{
    /// <summary>
    /// Custom Ads Configuration - Your Own!
    /// Manages AdMob settings for your game
    /// </summary>
    [CreateAssetMenu(fileName = "My Ads Config", menuName = "MyGame/Ads Configuration")]
    public class MyAdsConfig : ScriptableObject
    {
        [Header("=== AdMob IDs ===")]
        [Tooltip("Your AdMob App ID from AdMob Console")]
        public string androidAppId = "ca-app-pub-XXXXXXXX~YYYYYY";
        public string iosAppId = "ca-app-pub-XXXXXXXX~YYYYYY";
        
        [Space(10)]
        [Header("=== Banner Ads ===")]
        public bool useBanner = true;
        public string androidBannerId = "ca-app-pub-XXXXXXXX/BANNER";
        public string iosBannerId = "ca-app-pub-XXXXXXXX/BANNER";
        
        [Space(10)]
        [Header("=== Interstitial Ads ===")]
        public bool useInterstitial = true;
        public string androidInterstitialId = "ca-app-pub-XXXXXXXX/INTERSTITIAL";
        public string iosInterstitialId = "ca-app-pub-XXXXXXXX/INTERSTITIAL";
        
        [Tooltip("How many seconds to wait before first interstitial ad")]
        public float firstAdDelay = 40f;
        
        [Tooltip("Minimum seconds between interstitial ads")]
        public float adCooldown = 30f;
        
        [Space(10)]
        [Header("=== Rewarded Video Ads ===")]
        public bool useRewarded = true;
        public string androidRewardedId = "ca-app-pub-XXXXXXXX/REWARDED";
        public string iosRewardedId = "ca-app-pub-XXXXXXXX/REWARDED";
        
        [Space(10)]
        [Header("=== Advanced Settings ===")]
        [Tooltip("Load ads when game starts?")]
        public bool loadOnStartup = true;
        
        [Tooltip("Show ad loading message?")]
        public bool showLoadingMessage = false;
        public string loadingMessage = "Please wait...";
        
        // Helper methods for easy access
        public string GetAppId()
        {
#if UNITY_ANDROID
            return androidAppId;
#elif UNITY_IOS
            return iosAppId;
#else
            return androidAppId;
#endif
        }
        
        public string GetBannerId()
        {
#if UNITY_ANDROID
            return androidBannerId;
#elif UNITY_IOS
            return iosBannerId;
#else
            return androidBannerId;
#endif
        }
        
        public string GetInterstitialId()
        {
#if UNITY_ANDROID
            return androidInterstitialId;
#elif UNITY_IOS
            return iosInterstitialId;
#else
            return androidInterstitialId;
#endif
        }
        
        public string GetRewardedId()
        {
#if UNITY_ANDROID
            return androidRewardedId;
#elif UNITY_IOS
            return iosRewardedId;
#else
            return androidRewardedId;
#endif
        }
    }
}






