using System;
using UnityEngine;
using CrystalUnbolt;

[CreateAssetMenu(fileName = "RCProfile", menuName = "Game/Remote Config Profile")]
public class CrystalRCProfile : ScriptableObject
{
    [Header("General")]
    [Tooltip("Editor me har Play par fresh fetch (0s cache).")]
    public bool devForceFreshFetch = true;

    [Tooltip("Fetch/Bind ko thoda delay do takki first-frame smooth rahe.")]
    public float startDelaySeconds = 0.8f;

    // [Header("Link Monetization Settings (assign once)")]
    // public MonetizationSettings monetizationSettings; // Old system removed!

    [Header("Remote Config Keys - App IDs")]
    public string kAndroidAppId = "admob_android_app_id";
    public string kiOSAppId = "admob_ios_app_id";
    public string kLevelPlayAndroidKey = "levelplay_android_app_key";
    public string kLevelPlayiOSKey = "levelplay_ios_app_key";
    public string kUnityAdsAndroidId = "unity_ads_android_app_id";
    public string kUnityAdsiOSId = "unity_ads_ios_app_id";

    [Header("Remote Config Keys - Ads (Android)")]
    public string kBannerAndroid = "admob_banner_id_android";
    public string kInterstitialAndroid = "admob_interstitial_id_android";
    public string kRewardAndroid = "admob_reward_id_android";
    public string kAppOpenAndroid = "app_open_ads";

    [Header("Remote Config Keys - Ads (iOS)")]
    public string kBanneriOS = "admob_banner_id_ios";
    public string kInterstitialiOS = "admob_interstitial_id_ios";
    public string kRewardiOS = "admob_reward_id_ios";
    public string kAppOpeniOS = "app_open_ads_ios";

    [Header("Defaults (used if RC empty) - App IDs")]
    public string defAndroidAppId = "";
    public string defiOSAppId = "";
    public string defLevelPlayAndroidKey = "";
    public string defLevelPlayiOSKey = "";
    public string defUnityAdsAndroidId = "";
    public string defUnityAdsiOSId = "";

    [Header("Defaults (used if RC empty) - Ad IDs")]
    public string defBannerAndroid = "ca-app-pub-3940256099942544/6300978111";
    public string defInterstitialAndroid = "ca-app-pub-3940256099942544/1033173712";
    public string defRewardAndroid = "ca-app-pub-3940256099942544/5224354917";
    public string defAppOpenAndroid = "";
    public string defBanneriOS = "ca-app-pub-3940256099942544/2934735716";
    public string defInterstitialiOS = "ca-app-pub-3940256099942544/4411468910";
    public string defRewardiOS = "ca-app-pub-3940256099942544/1712485313";
    public string defAppOpeniOS = "";

    // IAP REMOVED - No in-app purchases in this version
    // [Header("Remote Config Keys - IAP (Android)")]
    // public string kNoAdsA = "CrystalUnboltLogic_noads";
    // public string kStarterA = "CrystalUnboltLogic_starter";
    // public string kGoldSmallA = "iap_goldsmall_android";
    // public string kGoldMediumA = "iap_goldmedium_android";
    // public string kGoldBigA = "iap_goldbig_android";
    // public string kPuPackA = "iap_pupack_android";

    // [Header("Defaults (used if RC empty)")]
    // public string defNoAdsA = "noads";
    // public string defStarterA = "starterpackage";
    // public string defGoldSmallA = "goldsmall";
    // public string defGoldMediumA = "goldmedium";
    // public string defGoldBigA = "goldbig";
    // public string defPuPackA = "pupack";

    // [Header("IAP: Product name hints (match by contains)")]
    // public string hintNoAds = "NoAds";
    // public string hintStarter = "Starter";
    // public string hintGoldSmall = "GoldSmall";
    // public string hintGoldMedium = "GoldMedium";
    // public string hintGoldBig = "GoldBig";
    // public string hintPuPack = "PUPack";
}
