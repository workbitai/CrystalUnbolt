using System.Collections.Generic;
using UnityEngine;

namespace CrystalUnbolt
{
    public sealed class AndroidHapticWrapper : BaseHapticWrapper
    {
        private Dictionary<int, AndroidHapticPattern> registeredPatterns;

        private AndroidJavaObject vibrationService;
        private AndroidJavaClass vibrationEffectClass;
        private int sdkVersion;
        private bool supportsAmplitudeControl;
        private bool supportsPredefinedEffects;
        private int effectTickId = -1;
        private int effectClickId = -1;
        private int effectHeavyClickId = -1;
        private static float lastUnityFallbackTime = -1f;
        private const float UNITY_FALLBACK_COOLDOWN = 0.5f;

        public override void Init()
        {
            registeredPatterns = new Dictionary<int, AndroidHapticPattern>();

#if UNITY_ANDROID
            Try(() =>
            {
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                    vibrationService = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                }

                // Get the Android SDK version
                using (AndroidJavaClass versionClass = new AndroidJavaClass("android.os.Build$VERSION"))
                {
                    sdkVersion = versionClass.GetStatic<int>("SDK_INT");
                }

                supportsAmplitudeControl = sdkVersion >= 26;
                supportsPredefinedEffects = sdkVersion >= 29;

                if (supportsAmplitudeControl)
                {
                    vibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect");

                    if (supportsPredefinedEffects && vibrationEffectClass != null)
                    {
                        Try(() =>
                        {
                            effectTickId = vibrationEffectClass.GetStatic<int>("EFFECT_TICK");
                            effectClickId = vibrationEffectClass.GetStatic<int>("EFFECT_CLICK");
                            effectHeavyClickId = vibrationEffectClass.GetStatic<int>("EFFECT_HEAVY_CLICK");
                        }, "Failed to cache predefined effect IDs!");
                    }
                }
            }, "Failed to Initialize haptic module!");
#endif
        }

        public override void Play(float duration = 0.3f, float intensity = 1.0f)
        {
            if (vibrationService == null)
            {
                TriggerUnityVibrationFallback(duration);
                return;
            }

#if UNITY_ANDROID
            LogAndroid($"OneShot requested | duration: {duration:0.###}s | intensity: {intensity:0.###} | sdk: {sdkVersion}");
            bool success = false;
            Try(() =>
            {
                if (supportsAmplitudeControl && vibrationEffectClass != null)
                {
                    using (AndroidJavaObject vibrationEffect = CreateAndroidEffect(duration, intensity))
                    {
                        if (vibrationEffect != null)
                        {
                            vibrationService.Call("vibrate", vibrationEffect);
                            success = true;
                            return;
                        }
                    }
                }

                vibrationService.Call("vibrate", (long)Mathf.Max(10f, duration * 1000f));
                success = true;
            }, "Failed to play haptic!");

            if (!success)
                TriggerUnityVibrationFallback(duration);
#endif
        }

        public override void Play(string patternID)
        {
            int patternHash = patternID.GetHashCode();

            if (!registeredPatterns.ContainsKey(patternHash))
            {
                TriggerUnityVibrationFallback(0.05f);
                return;
            }

            if (vibrationService == null)
            {
                TriggerUnityVibrationFallback(0.05f);
                return;
            }

#if UNITY_ANDROID
            LogAndroid($"Waveform requested | patternID: {patternID} | sdk: {sdkVersion}");
            bool success = false;
            Try(() =>
            {
                AndroidHapticPattern androidHapticPattern = registeredPatterns[patternHash];

                if (supportsAmplitudeControl && vibrationEffectClass != null)
                {
                    using (AndroidJavaObject vibrationEffect = vibrationEffectClass.CallStatic<AndroidJavaObject>("createWaveform", androidHapticPattern.Pattern, androidHapticPattern.Amplitudes, -1))
                    {
                        vibrationService.Call("vibrate", vibrationEffect);
                        success = true;
                    }
                }
                else
                {
                    vibrationService.Call("vibrate", androidHapticPattern.Pattern, -1);
                    success = true;
                }
            }, string.Format("Failed to play pattern with ID: {0}!", patternID));

            if (!success)
                TriggerUnityVibrationFallback(0.05f);
#endif
        }

        public override void RegisterPattern(HapticPattern pattern)
        {
            int patternHash = pattern.ID.GetHashCode();

            if (registeredPatterns.ContainsKey(patternHash)) return;

            registeredPatterns.Add(patternHash, new AndroidHapticPattern(pattern));
        }

        private class AndroidHapticPattern
        {
            public long[] Pattern { get; private set; }
            public int[] Amplitudes { get; private set; }

            public AndroidHapticPattern(HapticPattern hapticPattern)
            {
                List<long> patternList = new List<long>();
                List<int> amplitudeList = new List<int>();

                float previousEndTime = 0f;

                float durationMultiplier = Haptic.PlatformDurationMultiplier;

                foreach (HapticEvent hapticEvent in hapticPattern.Pattern)
                {
                    // Calculate the delay (pause) before the current haptic event
                    if (hapticEvent.StartTime > previousEndTime)
                    {
                        float pause = (hapticEvent.StartTime - previousEndTime) * durationMultiplier;
                        patternList.Add((long)(pause * 1000)); // convert to milliseconds
                        amplitudeList.Add(0); // no vibration during the pause
                    }

                    // Add the duration of the haptic event
                    long duration = (long)(hapticEvent.Duration * durationMultiplier * 1000); // convert to milliseconds
                    patternList.Add(duration);

                    // Convert intensity to a value between 0 and 255
                    float scaledIntensity = Mathf.Clamp01(hapticEvent.Intensity * Haptic.PlatformIntensityMultiplier);
                    int intensity = ToAndroidAmplitude(scaledIntensity);
                    amplitudeList.Add(intensity);

                    // Update previousEndTime
                    previousEndTime = hapticEvent.StartTime + hapticEvent.Duration;

                    Debug.Log($"Waveform event | patternID: {hapticPattern.ID} | start: {hapticEvent.StartTime:0.###}s | duration: {hapticEvent.Duration:0.###}s -> {duration}ms | intensity(raw): {hapticEvent.Intensity:0.###} | intensity(scaled 0-255): {intensity}");
                }

                // Convert lists to arrays
                Pattern = patternList.ToArray();
                Amplitudes = amplitudeList.ToArray();
            }
        }

        private void LogAndroid(string message)
        {
            if (!Haptic.VerboseLogging) return;

            Debug.Log($"[Haptic][Android] {message}");
        }

        private void TriggerUnityVibrationFallback(float suggestedDuration)
        {
#if UNITY_ANDROID
            float now = Time.realtimeSinceStartup;
            if (lastUnityFallbackTime > 0f && now - lastUnityFallbackTime < UNITY_FALLBACK_COOLDOWN)
            {
                LogAndroid($"Fallback suppressed to keep smoothness | delta: {now - lastUnityFallbackTime:0.###}s");
                return;
            }

            lastUnityFallbackTime = now;

            LogAndroid($"Fallback -> Handheld.Vibrate() | suggestedDuration: {suggestedDuration:0.###}s");
            Handheld.Vibrate();
#endif
        }

        private AndroidJavaObject CreateAndroidEffect(float duration, float intensity)
        {
            if (vibrationEffectClass == null) return null;

            float clampedDuration = Mathf.Max(0.01f, duration);
            float clampedIntensity = Mathf.Clamp01(intensity);

            if (supportsPredefinedEffects && clampedDuration <= 0.25f)
            {
                int effectId = effectTickId >= 0 ? effectTickId : ResolvePredefinedEffect(clampedIntensity);
                if (effectId >= 0)
                {
                    return vibrationEffectClass.CallStatic<AndroidJavaObject>("createPredefined", effectId);
                }
            }

            long durationMs = (long)(clampedDuration * 1000f);
            int amplitude = ToAndroidAmplitude(clampedIntensity);

            return vibrationEffectClass.CallStatic<AndroidJavaObject>("createOneShot", durationMs, amplitude);
        }

        private int ResolvePredefinedEffect(float intensity)
        {
            if (!supportsPredefinedEffects) return -1;

            // Keep everything in the light / normal range.
            // We never use EFFECT_HEAVY_CLICK to avoid “punchy” haptics.
            if (intensity <= 0.35f && effectTickId >= 0)
                return effectTickId;   // very light

            if (effectClickId >= 0)
                return effectClickId;  // medium

            // Fallback – still keep it light
            return effectTickId >= 0 ? effectTickId : -1;
        }


        private static int ToAndroidAmplitude(float normalizedIntensity)
        {
            // Lower overall range so Android feels softer
            const int minAmplitude = 1;
            const int maxAmplitude = 35;   // was 60

            // Make curve a bit “gentler” so low intensities stay very soft
            float eased = Mathf.SmoothStep(0f, 1f, normalizedIntensity);
            eased = Mathf.Pow(eased, 1.3f); // small tweak, keeps low values low

            int amplitude = Mathf.RoundToInt(Mathf.Lerp(minAmplitude, maxAmplitude, eased));
            return Mathf.Clamp(amplitude, minAmplitude, maxAmplitude);
        }

    }
}
