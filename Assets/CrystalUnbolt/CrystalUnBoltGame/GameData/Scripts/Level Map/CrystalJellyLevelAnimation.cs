using System.Collections;
using UnityEngine;
using DG.Tweening;
using DGEase = DG.Tweening.Ease;
using DOShortcuts = DG.Tweening.ShortcutExtensions;

namespace CrystalUnbolt.Map
{
    public class CrystalJellyLevelAnimation : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float jellyJumpHeight = 0.2f;
        [SerializeField] private float jellyJumpDuration = 0.20f;
        [SerializeField] private int jellyJumpCount = 3;

        [Header("Spin Animation")]
        [SerializeField] private float spinHeight = 0.9f;
        [SerializeField] private float spinDuration = 1.0f;
        [SerializeField] private float spinRotations = 2f;

        [Header("Fall / Bounce")]
        [SerializeField] private float fallDuration = 0.10f;   
        [SerializeField] private float fallBounceIntensity = 0.1f;

        [Header("Wait (only at END)")]
        [SerializeField] private float waitDuration = 1.5f;

        [Header("Animation Control")]
        [SerializeField] private bool autoStart = true;
        [SerializeField] private bool loopAnimation = true;

        [Header("Landing Particles")]
        [SerializeField] private ParticleSystem landingParticles;
        [SerializeField] private bool particleInitiallyInactive = true;

        [Header("Particles Toggle")]
        [Tooltip("Fall start par particles ON, bounce ke baad OFF.")]
        [SerializeField] private bool toggleParticlesDuringFall = true;
        [SerializeField] private bool hideParticlesAfterBounce = true;

        [Header("Follow Scroll")]
        [Tooltip("Local-space tweens so the object scrolls with its parent chunk.")]
        [SerializeField] private bool useLocalSpace = true;

        [Header("Speed")]
        [SerializeField] private float animationSpeed = 1f;  

        Vector3 basePos;
        Vector3 baseScale;
        Quaternion baseRot;

        Sequence seq;
        bool isAnimating;

        void Awake()
        {
            CaptureBaseFromCurrent();
            if (landingParticles && particleInitiallyInactive)
                landingParticles.gameObject.SetActive(false);
        }

        void Start()
        {
            if (autoStart) StartAnimation(); 
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

            for (int i = 0; i < jellyJumpCount; i++)
            {
                seq.Append(MoveY(GetBaseY() + jellyJumpHeight, jellyJumpDuration * 0.6f)
                           .SetEase(DGEase.OutQuad));
                seq.Join(Scale(baseScale * 1.1f, jellyJumpDuration * 0.6f)
                           .SetEase(DGEase.OutQuad));
                seq.Append(MoveY(GetBaseY(), jellyJumpDuration * 0.4f)
                           .SetEase(DGEase.InBounce));
                seq.Join(Scale(baseScale, jellyJumpDuration * 0.4f)
                           .SetEase(DGEase.InBounce));

                if (i < jellyJumpCount - 1) seq.AppendInterval(0.1f);
            }

            seq.Append(MoveY(GetBaseY() + spinHeight, spinDuration * 0.7f)
                       .SetEase(DGEase.OutQuad));
            seq.Join(Rotate(new Vector3(0, 0, 360f * spinRotations), spinDuration, RotateMode.FastBeyond360)
                    .SetEase(DGEase.InOutQuad));

            if (toggleParticlesDuringFall)
                seq.AppendCallback(() => SetLandingParticlesActive(true)); 

            var fallTween = MoveY(GetBaseY() - fallBounceIntensity, fallDuration)
                            .SetEase(DGEase.InQuart);
            fallTween.OnComplete(PlayLandingParticles); 
            seq.Append(fallTween);

            var bounceTween = MoveY(GetBaseY(), fallDuration * 0.5f)
                              .SetEase(DGEase.OutBounce);
            seq.Append(bounceTween);

            if (hideParticlesAfterBounce)
                seq.AppendCallback(() => SetLandingParticlesActive(false)); 

            seq.AppendInterval(waitDuration);

            seq.Append(Rotate(baseRot.eulerAngles, 0.20f, RotateMode.Fast)
                       .SetEase(DGEase.OutQuad));

            seq.timeScale = Mathf.Max(0.01f, animationSpeed);

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

        void PlayLandingParticles()
        {
            if (!landingParticles) return;
            if (!landingParticles.gameObject.activeSelf)
                landingParticles.gameObject.SetActive(true);
            landingParticles.Play(true);
        }

        void SetLandingParticlesActive(bool active)
        {
            if (!landingParticles) return;

            if (active)
            {
                if (!landingParticles.gameObject.activeSelf)
                    landingParticles.gameObject.SetActive(true);
                if (!landingParticles.isPlaying) landingParticles.Play(true);
            }
            else
            {
                landingParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                landingParticles.gameObject.SetActive(false);
            }
        }

        void OnDisable()
        {
            if (seq != null) seq.Pause(); 
        }

        void OnDestroy() { KillSeq(); }

        public void SetLoopAnimation(bool loop)
        {
            loopAnimation = loop;
          
        }

        public void SetAnimationSpeed(float speed)
        {
            animationSpeed = Mathf.Max(0.01f, speed);
            if (seq != null) seq.timeScale = animationSpeed;
        }

        public bool IsAnimating() => isAnimating;
    }
}
