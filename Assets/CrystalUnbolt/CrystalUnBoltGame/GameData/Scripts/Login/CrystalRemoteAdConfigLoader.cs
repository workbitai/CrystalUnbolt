/*using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.RemoteConfig;
using UnityEngine;

// FBInitOnce removed - using CrystalRCBootstrap.Ensure() as single source

public class CrystalRemoteAdConfigLoader : MonoBehaviour
{
    public static event Action OnApplied;

    [Header("Editor/Startup")]
    public float startDelay = 0.8f;
#if UNITY_EDITOR
    public bool devForceFreshFetchInEditor = true; // RC badalne par ON rakho
#endif

    [Header("Inspector Defaults (use while RC empty)")]
    public string bannerAndroidDefault = "test-banner";
    public string interAndroidDefault = "test-inter";
    public string rewardAndroidDefault = "test-reward";
    public string appOpenAndroidDefault = "";

    [Header("Remote Config Keys (Android)")]
    public string kBannerAndroid = "admob_banner_id_android";
    public string kInterstitialAndroid = "admob_interstitial_id_android";
    public string kRewardAndroid = "admob_reward_id_android";
    public string kAppOpenAndroid = "app_open_ads";

    // last read values (available to binder)
    public static string BannerA, InterA, RewardA, AppOpenA;

    void Start() => StartCoroutine(DelayThenRun());
    System.Collections.IEnumerator DelayThenRun()
    {
        if (startDelay > 0) yield return new WaitForSeconds(startDelay);
        _ = InitAndFetch();
    }

    public async Task InitAndFetch()
    {
        var dep = await CrystalRCBootstrap.Ensure();
        if (dep != DependencyStatus.Available)
        {
            UseInspectorDefaults();
            OnApplied?.Invoke();
            Debug.LogWarning($"[RC][ADS] Firebase deps: {dep}. Using inspector defaults.");
            return;
        }

#if UNITY_EDITOR
        // editor me cache 0 (har play me fresh)
        FirebaseRemoteConfig.DefaultInstance.Settings = new ConfigSettings
        {
            MinimumFetchIntervalInMilliseconds = devForceFreshFetchInEditor ? 0 : 12 * 60 * 60 * 1000
        };
#endif
        // defaults (so GetValue kabhi empty ho to bhi fallback mile)
        await FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(new Dictionary<string, object> {
            { kBannerAndroid,   bannerAndroidDefault },
            { kInterstitialAndroid, interAndroidDefault },
            { kRewardAndroid,   rewardAndroidDefault },
            { kAppOpenAndroid,  appOpenAndroidDefault },
        });

#if UNITY_EDITOR
        if (!devForceFreshFetchInEditor)
        {
            ReadValues();
            OnApplied?.Invoke();
            return;
        }
#endif
        try
        {
            await FirebaseRemoteConfig.DefaultInstance.FetchAsync();
            await FirebaseRemoteConfig.DefaultInstance.ActivateAsync();
        }
        catch (Exception e) { Debug.LogWarning($"[RC][ADS] Fetch err: {e.Message}"); }

        ReadValues();
        OnApplied?.Invoke();
    }

    void UseInspectorDefaults()
    {
        BannerA = bannerAndroidDefault;
        InterA = interAndroidDefault;
        RewardA = rewardAndroidDefault;
        AppOpenA = appOpenAndroidDefault;
    }

    void ReadValues()
    {
        string V(string key, string deflt)
        {
            var s = FirebaseRemoteConfig.DefaultInstance.GetValue(key).StringValue;
            return string.IsNullOrEmpty(s) ? deflt : s;
        }

        BannerA = V(kBannerAndroid, bannerAndroidDefault);
        InterA = V(kInterstitialAndroid, interAndroidDefault);
        RewardA = V(kRewardAndroid, rewardAndroidDefault);
        AppOpenA = V(kAppOpenAndroid, appOpenAndroidDefault);

        Debug.Log($"[RC][ADS] read ? banner={BannerA}, inter={InterA}, reward={RewardA}, appOpen={AppOpenA}");
    }
}
*/