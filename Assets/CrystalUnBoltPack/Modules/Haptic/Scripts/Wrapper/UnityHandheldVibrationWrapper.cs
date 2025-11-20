using UnityEngine;

namespace CrystalUnbolt
{
    public sealed class UnityHandheldVibrationWrapper : BaseHapticWrapper
    {
        private const float MIN_COOLDOWN = 0.25f;
        private float lastVibrationTime = -MIN_COOLDOWN;

        public override void Init()
        {
            lastVibrationTime = -MIN_COOLDOWN;
            Log("Unity handheld vibration initialized for Android.");
        }

        public override void Play(float duration = 0.3f, float intensity = 1.0f)
        {
            TriggerUnityVibration("duration");
        }

        public override void Play(string patternID)
        {
            TriggerUnityVibration($"pattern:{patternID}");
        }

        public override void RegisterPattern(HapticPattern pattern)
        {
            // Patterns are not used when relying on Handheld.Vibrate.
        }

        private void TriggerUnityVibration(string source)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            float now = Time.realtimeSinceStartup;

            if (lastVibrationTime > 0f && now - lastVibrationTime < MIN_COOLDOWN)
            {
                Log($"Unity vibrate suppressed ({source}) to keep smooth feedback.");
                return;
            }

            lastVibrationTime = now;

            Log($"Unity vibrate triggered ({source}).");
            Handheld.Vibrate();
#else
            Log($"Unity vibrate skipped ({source}) - unsupported platform.");
#endif
        }
    }
}

