using System.Collections;
using UnityEngine;
using DG.Tweening;
using DGEase = DG.Tweening.Ease;
using DOShortcuts = DG.Tweening.ShortcutExtensions;

namespace CrystalUnbolt.Map
{
    public class CrystalJellyLevelAnimation : MonoBehaviour
    {
        [Header("Animation Type")]
        [SerializeField] private AnimationType animationType = AnimationType.Pulse;

        [Header("Pulse Animation (Gentle Heartbeat)")]
        [SerializeField] private float pulseScaleMin = 1.0f;
        [SerializeField] private float pulseScaleMax = 1.08f;
        [SerializeField] private float pulseDuration = 0.4f; // Faster pulse
        [SerializeField] private int pulseCount = 2; // Number of pulses before pause

        [Header("Glow Animation (Scale + Slight Rotation)")]
        [SerializeField] private float glowScale = 1.2f;
        [SerializeField] private float glowDuration = 1.0f;
        [SerializeField] private float glowRotationAngle = 10f; // Slight tilt

        [Header("Bounce Animation (Vertical Bounce)")]
        [SerializeField] private float bounceHeight = 0.3f;
        [SerializeField] private float bounceDuration = 0.5f;
        [SerializeField] private int bounceCount = 2;

        [Header("Wait Between Cycles")]
        [SerializeField] private float waitDuration = 0.5f; // Shorter wait between cycles

        [Header("Animation Control")]
        [SerializeField] private bool autoStart = false; // Changed to false for grid usage
        [SerializeField] private bool loopAnimation = true;
        [SerializeField] private float delayBeforeStart = 0f; // Delay before animation starts

        public enum AnimationType
        {
            Pulse,      // Gentle heartbeat scale effect
            Glow,       // Scale up + slight rotation
            Bounce,     // Simple vertical bounce
            Float       // Gentle floating up and down
        }

        [Header("Follow Scroll")]
        [Tooltip("Local-space tweens so the object scrolls with its parent chunk.")]
        [SerializeField] private bool useLocalSpace = true;  

        Vector3 basePos;
        Vector3 baseScale;
        Quaternion baseRot;

        Sequence seq;
        bool isAnimating;

        void Awake()
        {
            CaptureBaseFromCurrent();
        }

        void Start()
        {
            if (autoStart)
            {
                if (delayBeforeStart > 0)
                    StartCoroutine(StartAnimationAfterDelay());
                else
                    StartAnimation();
            }
        }

        IEnumerator StartAnimationAfterDelay()
        {
            yield return new WaitForSeconds(delayBeforeStart);
            StartAnimation();
        }

        void OnEnable()
        {
            CaptureBaseFromCurrent();      
            if (isAnimating)
                StartCoroutine(RestartNextFrame());
        }

        IEnumerator RestartNextFrame()
        {
            yield return null;
            StartAnimation();
        }

        public void StartAnimation()
        {
            KillSeq();
            CaptureBaseFromCurrent();
            isAnimating = true;
            CreateSequence();
            seq.Play();
        }

        public void StopAnimation()
        {
            KillSeq();
            ResetToBase();
            isAnimating = false;
        }

        void KillSeq()
        {
            if (seq != null)
            {
                seq.Kill();
                seq = null;
            }
        }

        void CreateSequence()
        {
            seq = DOTween.Sequence();

            // Choose animation based on selected type
            switch (animationType)
            {
                case AnimationType.Pulse:
                    CreatePulseAnimation();
                    break;
                case AnimationType.Glow:
                    CreateGlowAnimation();
                    break;
                case AnimationType.Bounce:
                    CreateBounceAnimation();
                    break;
                case AnimationType.Float:
                    CreateFloatAnimation();
                    break;
            }

            seq.OnComplete(() =>
            {
                if (loopAnimation)
                {
                    CreateSequence();
                    seq.Play();
                }
                else
                {
                    isAnimating = false;
                }
            });
        }

        void CreatePulseAnimation()
        {
            // Gentle heartbeat pulse effect
            for (int i = 0; i < pulseCount; i++)
            {
                // Scale up (pump)
                seq.Append(Scale(baseScale * pulseScaleMax, pulseDuration * 0.5f)
                    .SetEase(DGEase.OutQuad));
                
                // Return to normal
                seq.Append(Scale(baseScale, pulseDuration * 0.5f)
                    .SetEase(DGEase.InOutQuad));

                if (i < pulseCount - 1)
                    seq.AppendInterval(0.1f); // Quick pause between pulses
            }

            seq.AppendInterval(waitDuration);
        }

        void CreateGlowAnimation()
        {
            // Scale up + slight rotation glow effect
            seq.Append(Scale(baseScale * glowScale, glowDuration * 0.5f)
                .SetEase(DGEase.OutCubic));
            seq.Join(Rotate(new Vector3(0, 0, baseRot.eulerAngles.z + glowRotationAngle), glowDuration * 0.5f, RotateMode.Fast)
                .SetEase(DGEase.OutCubic));

            // Hold
            seq.AppendInterval(0.3f);

            // Scale down + rotate back
            seq.Append(Scale(baseScale, glowDuration * 0.5f)
                .SetEase(DGEase.InCubic));
            seq.Join(Rotate(baseRot.eulerAngles, glowDuration * 0.5f, RotateMode.Fast)
                .SetEase(DGEase.InCubic));

            seq.AppendInterval(waitDuration);
        }

        void CreateBounceAnimation()
        {
            // Simple vertical bounce animation
            for (int i = 0; i < bounceCount; i++)
            {
                // Bounce up
                seq.Append(MoveY(GetBaseY() + bounceHeight, bounceDuration * 0.4f)
                    .SetEase(DGEase.OutQuad));
                
                // Fall down with bounce
                seq.Append(MoveY(GetBaseY(), bounceDuration * 0.6f)
                    .SetEase(DGEase.OutBounce));

                if (i < bounceCount - 1)
                    seq.AppendInterval(0.2f);
            }

            seq.AppendInterval(waitDuration);
        }

        void CreateFloatAnimation()
        {
            // Gentle floating up and down
            seq.Append(MoveY(GetBaseY() + 0.15f, 1.0f)
                .SetEase(DGEase.InOutSine));
            seq.Append(MoveY(GetBaseY() - 0.15f, 1.0f)
                .SetEase(DGEase.InOutSine));
            seq.Append(MoveY(GetBaseY(), 1.0f)
                .SetEase(DGEase.InOutSine));

            seq.AppendInterval(waitDuration * 0.5f);
        }

        void CaptureBaseFromCurrent()
        {
            baseScale = transform.localScale;
            baseRot = transform.rotation;
            basePos = useLocalSpace ? transform.localPosition : transform.position;
        }

        void ResetToBase()
        {
            if (useLocalSpace) transform.localPosition = basePos;
            else transform.position = basePos;

            transform.localScale = baseScale;
            transform.rotation = baseRot;
        }

        float GetBaseY() => basePos.y;

        Tweener MoveY(float y, float d)
        {
            return useLocalSpace
                ? DOShortcuts.DOLocalMoveY(transform, y, d, false)
                : DOShortcuts.DOMoveY(transform, y, d, false);
        }

        Tweener Scale(Vector3 s, float d) =>
            DOShortcuts.DOScale(transform, s, d);

        Tweener Rotate(Vector3 euler, float d, RotateMode mode) =>
            DOShortcuts.DORotate(transform, euler, d, mode);

        void OnDisable()
        {
            if (seq != null) seq.Pause(); 
        }

        void OnDestroy() { KillSeq(); }

        public void SetLoopAnimation(bool loop)
        {
            loopAnimation = loop;
        }

        public void SetAnimationType(AnimationType type)
        {
            if (animationType != type)
            {
                animationType = type;
                if (isAnimating)
                {
                    // Restart with new animation type
                    StartAnimation();
                }
            }
        }

        public bool IsAnimating() => isAnimating;
    }
}
