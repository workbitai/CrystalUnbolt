using System;
using UnityEngine;

/// <summary>
/// Lightweight replacement for the legacy monetization settings asset so the
/// remote-config sync scripts can keep working even though the original plugin
/// was removed.
/// </summary>
[CreateAssetMenu(fileName = "MonetizationSettings", menuName = "Game/Monetization Settings")]
public class MonetizationSettings : ScriptableObject
{
    public AdsSettings AdsSettings = new AdsSettings();
    public IAPSettings IAPSettings = new IAPSettings();
}

[Serializable]
public class AdsSettings
{
    public AdMobSettings AdMobContainer = new AdMobSettings();
    public LevelPlaySettings LevelPlayContainer = new LevelPlaySettings();
    public UnityAdsSettings UnityAdsContainer = new UnityAdsSettings();
}

[Serializable]
public class AdMobSettings
{
    // App IDs
    public string androidAppId;
    public string iosAppId;

    // Android unit ids (multiple spellings for reflection setter)
    public string androidBannerId;
    public string iOSBannerId;

    public string androidInterstitialId;
    public string iOSInterstitialId;

    public string androidRewardedId;
    public string iOSRewardedId;

}

[Serializable]
public class LevelPlaySettings
{
    public string androidAppKey;
    public string iOSAppKey;
}

[Serializable]
public class UnityAdsSettings
{
    public string androidAppID;
    public string iOSAppID;
}

[Serializable]
public class IAPSettings
{
    public IAPStoreItem[] storeItems = Array.Empty<IAPStoreItem>();
}

[Serializable]
public class IAPStoreItem
{
    public string productKeyType;
    public string androidID;
    public string androidId;
    public string iosID;
    public string iOSID;
    public string iosId;
    public string iOSId;
}

