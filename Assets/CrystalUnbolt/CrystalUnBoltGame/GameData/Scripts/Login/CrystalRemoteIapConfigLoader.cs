/*using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.RemoteConfig;
using UnityEngine;

public class CrystalRemoteIapConfigLoader : MonoBehaviour
{
    public static event Action OnRemoteIapApplied;

    [Header("Startup")]
    public float startDelaySeconds = 1.2f;
    public bool applyDefaultsOnStart = true;

#if UNITY_EDITOR
    [Header("Editor Dev")]
    public bool devForceFreshFetchInEditor = false;
#endif

    [Header("RC Keys")]
    public string kIapEnabled    = "iap_enabled";
    public string kUseFakeStore  = "iap_use_fake_store";
    public string kFakeStoreMode = "iap_fake_store_mode";

    public string kNoAdsAndroid      = "iap_noads_android";
    public string kNoAdsiOS          = "iap_noads_ios";
    public string kGoldSmallAndroid  = "iap_goldsmall_android";
    public string kGoldSmallIOS      = "iap_goldsmall_ios";
    public string kGoldMediumAndroid = "iap_goldmedium_android";
    public string kGoldMediumIOS     = "iap_goldmedium_ios";
    public string kGoldBigAndroid    = "iap_goldbig_android";
    public string kGoldBigIOS        = "iap_goldbig_ios";
    public string kPowerupPackAndroid= "iap_pupack_android";
    public string kPowerupPackIOS    = "iap_pupack_ios";

    private bool _started;

    private void Start()
    {
        if (_started) return;
        _started = true;
        StartCoroutine(DelayedStart());
    }

    private System.Collections.IEnumerator DelayedStart()
    {
        if (startDelaySeconds > 0f)
            yield return new WaitForSeconds(startDelaySeconds);
        _ = InitializeAndFetch();
    }

    private Dictionary<string, object> BuildDefaults() => new()
    {
        { kIapEnabled, true },
        { kUseFakeStore, false },
        { kFakeStoreMode, 0 },
        { kNoAdsAndroid, "noads_android" }, { kNoAdsiOS, "noads_ios" },
        { kGoldSmallAndroid, "gold_small_android" }, { kGoldSmallIOS, "gold_small_ios" },
        { kGoldMediumAndroid, "gold_medium_android" }, { kGoldMediumIOS, "gold_medium_ios" },
        { kGoldBigAndroid, "gold_big_android" }, { kGoldBigIOS, "gold_big_ios" },
        { kPowerupPackAndroid, "powerup_pack_android" }, { kPowerupPackIOS, "powerup_pack_ios" },
    };

    public async Task InitializeAndFetch()
    {
        var dep = await FirebaseBootstrap.Ensure();
        if (dep != DependencyStatus.Available)
        {
            Debug.LogWarning($"[RemoteIapConfig] Firebase dep not available: {dep}. Using defaults.");
            SeedDefaults();
            ApplyFromCurrent();
            return;
        }

        if (applyDefaultsOnStart)
            await FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(BuildDefaults());

#if UNITY_EDITOR
        if (!devForceFreshFetchInEditor)
        {
            ReadFromRCIntoCurrent();
            ApplyFromCurrent();
            return;
        }
#endif

        try
        {
            await FirebaseRemoteConfig.DefaultInstance.FetchAsync();
            await FirebaseRemoteConfig.DefaultInstance.ActivateAsync();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[RemoteIapConfig] Fetch error: {e.Message}");
        }

        ReadFromRCIntoCurrent();
        ApplyFromCurrent();
    }

    private void SeedDefaults()
    {
        RemoteIapIds.Current = new RemoteIapIds
        {
            iapEnabled = true,
            useFakeStore = false,
            fakeStoreMode = 0
        };
        RemoteIapIds.Current.Set("NoAds",      "noads_android",         "noads_ios");
        RemoteIapIds.Current.Set("GoldSmall",  "gold_small_android",    "gold_small_ios");
        RemoteIapIds.Current.Set("GoldMedium", "gold_medium_android",   "gold_medium_ios");
        RemoteIapIds.Current.Set("GoldBig",    "gold_big_android",      "gold_big_ios");
        RemoteIapIds.Current.Set("PowerUpPack","powerup_pack_android",  "powerup_pack_ios");
    }

    private void ReadFromRCIntoCurrent()
    {
        var rc = FirebaseRemoteConfig.DefaultInstance;

        string V(string key, string fallback)
        {
            var s = rc.GetValue(key).StringValue;
            return string.IsNullOrEmpty(s) ? fallback : s;
        }

        RemoteIapIds.Current = new RemoteIapIds
        {
            iapEnabled   = rc.GetValue(kIapEnabled).BooleanValue,
            useFakeStore = rc.GetValue(kUseFakeStore).BooleanValue,
            fakeStoreMode = (int)rc.GetValue(kFakeStoreMode).LongValue
        };

        RemoteIapIds.Current.Set("NoAds",      V(kNoAdsAndroid, "noads_android"),            V(kNoAdsiOS, "noads_ios"));
        RemoteIapIds.Current.Set("GoldSmall",  V(kGoldSmallAndroid, "gold_small_android"),   V(kGoldSmallIOS, "gold_small_ios"));
        RemoteIapIds.Current.Set("GoldMedium", V(kGoldMediumAndroid,"gold_medium_android"),  V(kGoldMediumIOS,"gold_medium_ios"));
        RemoteIapIds.Current.Set("GoldBig",    V(kGoldBigAndroid,   "gold_big_android"),     V(kGoldBigIOS,   "gold_big_ios"));
        RemoteIapIds.Current.Set("PowerUpPack",V(kPowerupPackAndroid,"powerup_pack_android"),V(kPowerupPackIOS,"powerup_pack_ios"));
    }

    private void ApplyFromCurrent()
    {
        try { OnRemoteIapApplied?.Invoke(); } catch { }
    }
}

public class RemoteIapIds
{
    public bool iapEnabled;
    public bool useFakeStore;
    public int fakeStoreMode;

    // key alias -> (android, ios)
    public readonly Dictionary<string, (string android, string ios)> map =
        new Dictionary<string, (string, string)>(StringComparer.OrdinalIgnoreCase);

    public static RemoteIapIds Current = new RemoteIapIds();

    public void Set(string alias, string android, string ios) =>
        map[alias] = (android, ios);

    public (string android, string ios) Get(string alias) =>
        map.TryGetValue(alias, out var v) ? v : default;
}
*/