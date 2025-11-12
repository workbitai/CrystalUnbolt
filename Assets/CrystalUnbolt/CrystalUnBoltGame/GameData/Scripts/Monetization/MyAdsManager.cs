using UnityEngine;
using GoogleMobileAds.Api;
using System;

namespace CrystalUnbolt
{
    /// <summary>
    /// Your Custom Ads Manager - Simple & Clean!
    /// Handles AdMob Banner, Interstitial, Rewarded ads
    /// </summary>
    public class MyAdsManager : MonoBehaviour
    {
        public static MyAdsManager Instance { get; private set; }
        
        [Header("Configuration")]
        [SerializeField] private MyAdsConfig adsConfig;
        
        private BannerView bannerView;
        private InterstitialAd interstitialAd;
        private RewardedAd rewardedAd;
        
        private float lastInterstitialTime;
        private bool isInitialized = false;
        
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        private void Start()
        {
            if (adsConfig != null && adsConfig.loadOnStartup)
            {
                InitializeAds();
            }
        }
        
        /// <summary>
        /// Initialize AdMob
        /// </summary>
        public void InitializeAds()
        {
            if (isInitialized) return;
            
            Debug.Log("[MyAds] Initializing AdMob...");
            
            MobileAds.Initialize(initStatus =>
            {
                Debug.Log("[MyAds] AdMob Initialized!");
                isInitialized = true;
                
                if (adsConfig.useBanner) LoadBanner();
                if (adsConfig.useInterstitial) LoadInterstitial();
                if (adsConfig.useRewarded) LoadRewarded();
            });
        }
        
        #region Banner
        
        public void LoadBanner()
        {
            if (!adsConfig.useBanner) return;
            
            string adUnitId = adsConfig.GetBannerId();
            
            if (bannerView != null) bannerView.Destroy();
            
            bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Bottom);
            
            AdRequest request = new AdRequest();
            bannerView.LoadAd(request);
            
            Debug.Log("[MyAds] Banner loaded!");
            Debug.Log("Addd");
        }
        
        public void ShowBanner()
        {
            if (bannerView != null)
            {
                bannerView.Show();
                Debug.Log("[MyAds] Banner shown!");
            }
        }
        
        public void HideBanner()
        {
            if (bannerView != null)
            {
                bannerView.Hide();
                Debug.Log("[MyAds] Banner hidden!");
            }
        }
        
        #endregion
        
        #region Interstitial
        
        public void LoadInterstitial()
        {
            if (!adsConfig.useInterstitial) return;
            
            string adUnitId = adsConfig.GetInterstitialId();
            
            InterstitialAd.Load(adUnitId, new AdRequest(),
                (InterstitialAd ad, LoadAdError error) =>
                {
                    if (error != null)
                    {
                        Debug.LogError("[MyAds] Interstitial failed to load: " + error);
                        return;
                    }
                    
                    interstitialAd = ad;
                    Debug.Log("[MyAds] Interstitial loaded!");
                });
        }
        
        public void ShowInterstitial(Action onComplete = null)
        {
            if (!adsConfig.useInterstitial)
            {
                onComplete?.Invoke();
                return;
            }
            
            // Check cooldown
            if (Time.time - lastInterstitialTime < adsConfig.adCooldown)
            {
                Debug.Log("[MyAds] Interstitial on cooldown!");
                onComplete?.Invoke();
                return;
            }
            
            if (interstitialAd != null && interstitialAd.CanShowAd())
            {
                interstitialAd.Show();
                lastInterstitialTime = Time.time;
                
                interstitialAd.OnAdFullScreenContentClosed += () =>
                {
                    LoadInterstitial(); // Reload next one
                    onComplete?.Invoke();
                };
                
                Debug.Log("[MyAds] Interstitial shown!");
            }
            else
            {
                Debug.Log("[MyAds] Interstitial not ready!");
                LoadInterstitial();
                onComplete?.Invoke();
            }
        }
        
        #endregion
        
        #region Rewarded
        
        public void LoadRewarded()
        {
            if (!adsConfig.useRewarded) return;
            
            string adUnitId = adsConfig.GetRewardedId();
            
            RewardedAd.Load(adUnitId, new AdRequest(),
                (RewardedAd ad, LoadAdError error) =>
                {
                    if (error != null)
                    {
                        Debug.LogError("[MyAds] Rewarded failed to load: " + error);
                        return;
                    }
                    
                    rewardedAd = ad;
                    Debug.Log("[MyAds] Rewarded loaded!");
                });
        }
        
        public void ShowRewarded(Action<bool> onComplete)
        {
            if (!adsConfig.useRewarded)
            {
                onComplete?.Invoke(false);
                return;
            }
            
            if (rewardedAd != null && rewardedAd.CanShowAd())
            {
                bool rewarded = false;
                
                // Register callbacks before showing
                rewardedAd.OnAdFullScreenContentClosed += () =>
                {
                    LoadRewarded(); // Reload next one
                    onComplete?.Invoke(rewarded);
                };
                
                // Show with reward callback
                rewardedAd.Show((Reward reward) =>
                {
                    rewarded = true;
                    Debug.Log($"[MyAds] User earned reward: {reward.Amount} {reward.Type}");
                });
                
                Debug.Log("[MyAds] Rewarded shown!");
            }
            else
            {
                Debug.Log("[MyAds] Rewarded not ready!");
                LoadRewarded();
                onComplete?.Invoke(false);
            }
        }
        
        #endregion
    }
}

