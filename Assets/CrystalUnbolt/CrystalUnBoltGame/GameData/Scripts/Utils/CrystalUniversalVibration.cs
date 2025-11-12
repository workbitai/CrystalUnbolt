using UnityEngine;

namespace CrystalUnbolt
{
    /// <summary>
    /// Universal Vibration System that supports all platforms (Android, iOS, Editor)
    /// This script provides a unified interface for vibration across all devices
    /// </summary>
    public static class CrystalUniversalVibration
    {
        // Vibration types with predefined values
        public enum VibrationType
        {
            Light = 0,      // Short, gentle vibration
            Medium = 1,     // Medium intensity vibration  
            Hard = 2,       // Strong vibration
            Custom = 3      // Custom duration and intensity
        }

        // Static settings
        private static bool isEnabled = true;
        private static bool verboseLogging = false;

        /// <summary>
        /// Enable or disable vibration globally
        /// </summary>
        public static bool IsEnabled
        {
            get { return isEnabled; }
            set 
            { 
                isEnabled = value;
                if (verboseLogging)
                    Debug.Log($"[UniversalVibration] Vibration {(value ? "enabled" : "disabled")}");
            }
        }

        /// <summary>
        /// Enable verbose logging for debugging
        /// </summary>
        public static bool VerboseLogging
        {
            get { return verboseLogging; }
            set { verboseLogging = value; }
        }

        /// <summary>
        /// Play vibration with predefined type
        /// </summary>
        /// <param name="type">Vibration type (Light, Medium, Hard)</param>
        public static void Vibrate(VibrationType type)
        {
            if (!isEnabled) return;

            switch (type)
            {
                case VibrationType.Light:
                    PlayVibration(0.05f, 0.3f);
                    break;
                case VibrationType.Medium:
                    PlayVibration(0.1f, 0.6f);
                    break;
                case VibrationType.Hard:
                    PlayVibration(0.15f, 0.9f);
                    break;
                default:
                    if (verboseLogging)
                        Debug.LogWarning($"[UniversalVibration] Invalid vibration type: {type}");
                    break;
            }
        }

        /// <summary>
        /// Play custom vibration with duration and intensity
        /// </summary>
        /// <param name="duration">Duration in seconds (0.01f to 1.0f)</param>
        /// <param name="intensity">Intensity from 0.0f to 1.0f</param>
        public static void Vibrate(float duration, float intensity = 1.0f)
        {
            if (!isEnabled) return;

            // Clamp values to safe ranges
            duration = Mathf.Clamp(duration, 0.01f, 1.0f);
            intensity = Mathf.Clamp(intensity, 0.0f, 1.0f);

            PlayVibration(duration, intensity);
        }

        /// <summary>
        /// Internal method that handles platform-specific vibration
        /// </summary>
        private static void PlayVibration(float duration, float intensity)
        {
            try
            {
#if UNITY_EDITOR
                // Editor vibration (simulation)
                if (verboseLogging)
                    Debug.Log($"[UniversalVibration] Editor: Playing vibration - Duration: {duration}s, Intensity: {intensity}");
                
#elif UNITY_ANDROID
                PlayAndroidVibration(duration, intensity);
                
#elif UNITY_IOS
                PlayIOSVibration(duration, intensity);
                
#else
                if (verboseLogging)
                    Debug.LogWarning("[UniversalVibration] Platform not supported for vibration");
#endif
            }
            catch (System.Exception e)
            {
                if (verboseLogging)
                    Debug.LogError($"[UniversalVibration] Failed to play vibration: {e.Message}");
            }
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        /// <summary>
        /// Android-specific vibration implementation
        /// </summary>
        private static void PlayAndroidVibration(float duration, float intensity)
        {
            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var context = currentActivity.Call<AndroidJavaObject>("getApplicationContext"))
                using (var vibrator = context.Call<AndroidJavaObject>("getSystemService", "vibrator"))
                {
                    if (vibrator == null)
                    {
                        if (verboseLogging)
                            Debug.LogWarning("[UniversalVibration] Android vibrator service not available");
                        return;
                    }

                    // Get Android SDK version
                    int sdk = new AndroidJavaClass("android.os.Build$VERSION").GetStatic<int>("SDK_INT");
                    long ms = Mathf.Max(1, Mathf.RoundToInt(duration * 1000f));

                    if (sdk >= 26) // Android 8.0 (API level 26) and above
                    {
                        using (var vibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect"))
                        {
                            // Convert intensity to amplitude (1-255)
                            int amplitude = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(40f, 255f, intensity)), 1, 255);
                            var effect = vibrationEffectClass.CallStatic<AndroidJavaObject>("createOneShot", ms, amplitude);
                            vibrator.Call("vibrate", effect);
                        }
                    }
                    else // Android 7.1 and below
                    {
                        vibrator.Call("vibrate", ms);
                    }

                    if (verboseLogging)
                        Debug.Log($"[UniversalVibration] Android: Playing vibration - Duration: {duration}s, Intensity: {intensity}");
                }
            }
            catch (System.Exception e)
            {
                if (verboseLogging)
                    Debug.LogError($"[UniversalVibration] Android vibration failed: {e.Message}");
            }
        }
#endif

#if UNITY_IOS && !UNITY_EDITOR
        /// <summary>
        /// iOS-specific vibration implementation using Core Haptics
        /// </summary>
        private static void PlayIOSVibration(float duration, float intensity)
        {
            try
            {
                // Check if Core Haptics is available (iOS 13+)
                if (SystemInfo.operatingSystem.StartsWith("iPhone OS 13") || 
                    SystemInfo.operatingSystem.StartsWith("iPhone OS 14") ||
                    SystemInfo.operatingSystem.StartsWith("iPhone OS 15") ||
                    SystemInfo.operatingSystem.StartsWith("iPhone OS 16") ||
                    SystemInfo.operatingSystem.StartsWith("iPhone OS 17") ||
                    SystemInfo.operatingSystem.StartsWith("iPhone OS 18"))
                {
                    // Use Core Haptics for iOS 13+
                    if (Haptic.IsInitialized && Haptic.IsActive)
                    {
                        Haptic.Play(duration, intensity);
                        if (verboseLogging)
                            Debug.Log($"[UniversalVibration] iOS Core Haptics: Playing vibration - Duration: {duration}s, Intensity: {intensity}");
                    }
                    else
                    {
                        // Fallback to system vibration
                        PlayIOSSystemVibration();
                        if (verboseLogging)
                            Debug.Log("[UniversalVibration] iOS Core Haptics not available, using system vibration");
                    }
                }
                else
                {
                    // Use system vibration for older iOS versions
                    PlayIOSSystemVibration();
                    if (verboseLogging)
                        Debug.Log("[UniversalVibration] iOS: Using system vibration (iOS < 13)");
                }
            }
            catch (System.Exception e)
            {
                if (verboseLogging)
                    Debug.LogError($"[UniversalVibration] iOS vibration failed: {e.Message}");
            }
        }

        /// <summary>
        /// iOS system vibration fallback
        /// </summary>
        private static void PlayIOSSystemVibration()
        {
            try
            {
                // Use Unity's Handheld.Vibrate() as fallback
                Handheld.Vibrate();
                if (verboseLogging)
                    Debug.Log("[UniversalVibration] iOS: System vibration played");
            }
            catch (System.Exception e)
            {
                if (verboseLogging)
                    Debug.LogError($"[UniversalVibration] iOS system vibration failed: {e.Message}");
            }
        }
#endif

        /// <summary>
        /// Quick vibration methods for common use cases
        /// </summary>
        public static void LightVibration() => Vibrate(VibrationType.Light);
        public static void MediumVibration() => Vibrate(VibrationType.Medium);
        public static void HardVibration() => Vibrate(VibrationType.Hard);

        /// <summary>
        /// Check if vibration is supported on current platform
        /// </summary>
        public static bool IsSupported()
        {
#if UNITY_ANDROID || UNITY_IOS
            return true;
#else
            return false;
#endif
        }

        /// <summary>
        /// Get platform name for debugging
        /// </summary>
        public static string GetPlatform()
        {
#if UNITY_EDITOR
            return "Editor";
#elif UNITY_ANDROID
            return "Android";
#elif UNITY_IOS
            return "iOS";
#else
            return "Unknown";
#endif
        }
    }
}
