using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TweenEase = DG.Tweening.Ease;
using TweenType = DG.Tweening.Tween;

namespace CrystalUnbolt
{
    [RequireComponent(typeof(RectTransform))]
    public class UIButtonClickFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        [Header("Press")]
        [SerializeField] private Vector3 pressedScale = new Vector3(0.92f, 0.92f, 0.92f);
        [SerializeField] private float pressDuration = 0.08f;
        [SerializeField] private TweenEase pressEase = TweenEase.InOutSine;

        [Header("Release Bounce")]
        [SerializeField] private Vector3 overshootScale = new Vector3(1.12f, 1.12f, 1f);
        [SerializeField] private float overshootDuration = 0.07f;
        [SerializeField] private Vector3 reboundScale = new Vector3(0.98f, 0.98f, 1f);
        [SerializeField] private float reboundDuration = 0.09f;
        [SerializeField] private float settleDuration = 0.08f;
        [SerializeField] private TweenEase releaseEase = TweenEase.OutSine;

        [Header("Options")]
        [SerializeField] private bool resetScaleOnEnable = true;
        [SerializeField] private bool playClickSound = false;
        [SerializeField] private AudioClip clickSound;
        [SerializeField] private bool hideAfterClick = false;
        [SerializeField] private float hideDelay = 0.25f;
        [SerializeField] private float hideDuration = 0.2f;

        private RectTransform rectTransform;
        private Vector3 defaultScale;
        private TweenType tween;
        private Sequence releaseSequence;
        private Coroutine hideRoutine;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            defaultScale = rectTransform.localScale;
        }

        private void OnEnable()
        {
            if (resetScaleOnEnable)
                ResetScale();
        }

        private void OnDisable()
        {
            KillTween();
            rectTransform.localScale = defaultScale;
            if (hideRoutine != null)
            {
                StopCoroutine(hideRoutine);
                hideRoutine = null;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            AnimateScale(pressedScale, pressDuration, pressEase);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            PlayReleaseBounce();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (playClickSound && clickSound != null)
                SoundManager.PlaySound(clickSound);

            if (hideAfterClick)
            {
                if (hideRoutine != null)
                    StopCoroutine(hideRoutine);
                hideRoutine = StartCoroutine(HideAfterDelay());
            }
        }

        private void AnimateScale(Vector3 target, float duration, TweenEase ease)
        {
            KillTween();
            tween = DG.Tweening.ShortcutExtensions
                .DOScale(rectTransform, target, duration)
                .SetEase(ease)
                .SetUpdate(true);
        }

        private void PlayReleaseBounce()
        {
            KillTween();

            releaseSequence = DOTween.Sequence().SetUpdate(true);
            releaseSequence.Append(DG.Tweening.ShortcutExtensions
                .DOScale(rectTransform, overshootScale, overshootDuration)
                .SetEase(releaseEase));
            releaseSequence.Append(DG.Tweening.ShortcutExtensions
                .DOScale(rectTransform, reboundScale, reboundDuration)
                .SetEase(releaseEase));
            releaseSequence.Append(DG.Tweening.ShortcutExtensions
                .DOScale(rectTransform, defaultScale, settleDuration)
                .SetEase(releaseEase));
        }

        private void KillTween()
        {
            if (tween != null)
                tween.Kill();

            tween = null;

            if (releaseSequence != null)
                releaseSequence.Kill();

            releaseSequence = null;
        }

        private void ResetScale()
        {
            KillTween();
            rectTransform.localScale = defaultScale;
            if (hideRoutine != null)
            {
                StopCoroutine(hideRoutine);
                hideRoutine = null;
            }
        }

        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSeconds(hideDelay);

            KillTween();
            DG.Tweening.ShortcutExtensions.DOScale(rectTransform, Vector3.zero, hideDuration)
                .SetEase(TweenEase.InBack)
                .SetUpdate(true);

            hideRoutine = null;
        }
    }
}

