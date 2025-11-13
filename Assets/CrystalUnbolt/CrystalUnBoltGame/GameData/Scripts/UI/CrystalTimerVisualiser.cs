using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace CrystalUnbolt
{
    public class CrystalTimerVisualiser : MonoBehaviour
    {
        [SerializeField] TMP_Text timerText;
        private CrystalGameTimer timer;

        [SerializeField] CrystalSlicedFilledImage fillImage;
        
        [Header("Scale Animation")]
        [SerializeField] GameObject scaleObject;
        [SerializeField] float scaleUpDuration = 1f;
        [SerializeField] float waitDuration = 0.5f;
        [SerializeField] float scaleDownDuration = 0.5f;
        [SerializeField] float targetScale = 10f;
        
        [Header("Shake Effect")]
        [SerializeField] float shakeDuration = 0.3f;
        [SerializeField] float shakeIntensity = 0.1f;
        [SerializeField] float lastSecondsThreshold = 10f; 
        [SerializeField] float continuousShakeIntensity = 0.05f;
        
        [Header("Level Restrictions")]
        [SerializeField] int[] excludedLevels = {2, 5, 12, 20}; 
        
        private bool isContinuousShaking = false;
        private Coroutine continuousShakeCoroutine;
        private float lastVibrationTime = 0f;

        public void Show(CrystalGameTimer timer)
        {
            this.timer = timer;

            gameObject.SetActive(true);

            timer.OnTimeSpanChanged += OnTimeChanged;

            OnTimeChanged(timer.CurrentTimeSpan);
            
            if (ShouldPlayAnimation())
            {
                StartScaleAnimation();
            }
        }

        private void OnDestroy()
        {
            if (timer != null)
                timer.OnTimeSpanChanged -= OnTimeChanged;
        }

        public void Hide()
        {
            gameObject.SetActive(false);

            if (timer != null)
                timer.OnTimeSpanChanged -= OnTimeChanged;
                
            StopContinuousShake();
        }

        public void SetFreezeFillAmount(float t)
        {
            fillImage.fillAmount = t;
        }

        public void OnTimeChanged(TimeSpan timeSpan)
        {
            timerText.text = string.Format("{0:mm\\:ss}", timeSpan);
            
            if (timer != null && timer.CurrentTime <= lastSecondsThreshold)
            {
                if (!isContinuousShaking)
                {
                    StartContinuousShake();
                }
            }
        }

        private bool ShouldPlayAnimation()
        {
            int currentLevel = CrystalLevelController.DisplayedLevelIndex + 1; 
            
            for (int i = 0; i < excludedLevels.Length; i++)
            {
                if (currentLevel == excludedLevels[i])
                {
                    Debug.Log($"[TimerVisual] Animation skipped for Level {currentLevel}");
                    return false;
                }
            }
            
            Debug.Log($"[TimerVisual] Animation will play for Level {currentLevel}");
            return true;
        }

        public void StartScaleAnimation()
        {
            if (scaleObject != null)
            {
                StartCoroutine(ScaleAnimationCoroutine());
            }
        }

        private IEnumerator ScaleAnimationCoroutine()
        {
            if (scaleObject == null) yield break;

            Vector3 originalScale = scaleObject.transform.localScale;
            Vector3 startScale = originalScale * 0.5f; // Start at 50% (less extreme)
            Vector3 maxScale = originalScale * 1.4f; // MAX SCALE: Only 1.4x (even smaller to avoid overlap)
            
            // Make sure timer text renders on top by setting it as last sibling
            if (timerText != null)
                timerText.transform.SetAsLastSibling();
            
            // Set initial small scale
            scaleObject.transform.localScale = startScale;
            
            // PHASE 1: Quick bounce from small to slightly bigger
            float elapsedTime = 0f;
            Vector3 mediumScale = originalScale * 1.15f; // More subtle
            
            while (elapsedTime < 0.3f)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / 0.3f;
                
                // Overshoot slightly then settle
                float overshoot = Mathf.Sin(progress * Mathf.PI);
                float scale = Mathf.Lerp(0.5f, 1.15f, progress) + overshoot * 0.1f;
                
                scaleObject.transform.localScale = originalScale * scale;
                
                yield return null;
            }
            
            // PHASE 2: Smooth grow to max size (1.8x) with gentle bounce
            elapsedTime = 0f;
            while (elapsedTime < scaleUpDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / scaleUpDuration;
                
                // Ease out back (slight overshoot)
                float easeProgress = progress < 0.5f 
                    ? 2f * progress * progress 
                    : 1f - Mathf.Pow(-2f * progress + 2f, 2f) / 2f;
                
                scaleObject.transform.localScale = Vector3.Lerp(mediumScale, maxScale, easeProgress);
                
                yield return null;
            }
            
            scaleObject.transform.localScale = maxScale;
            
            // PHASE 3: Hold at max size
            yield return new WaitForSeconds(waitDuration);
            
            // PHASE 4: Shrink back smoothly
            elapsedTime = 0f;
            while (elapsedTime < scaleDownDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / scaleDownDuration;
                
                // Smooth cubic ease out
                float smoothProgress = 1f - Mathf.Pow(1f - progress, 3f);
                scaleObject.transform.localScale = Vector3.Lerp(maxScale, originalScale, smoothProgress);
                
                yield return null;
            }

            scaleObject.transform.localScale = originalScale;
            
            StartCoroutine(ShakeEffectCoroutine());
        }

        private IEnumerator ShakeEffectCoroutine()
        {
            if (scaleObject == null) yield break;

            Vector3 originalPosition = scaleObject.transform.position;
            float elapsedTime = 0f;

            while (elapsedTime < shakeDuration)
            {
                elapsedTime += Time.deltaTime;
                
                float shakeX = UnityEngine.Random.Range(-shakeIntensity, shakeIntensity);
                float shakeY = UnityEngine.Random.Range(-shakeIntensity, shakeIntensity);
                
                Vector3 shakeOffset = new Vector3(shakeX, shakeY, 0f);
                scaleObject.transform.position = originalPosition + shakeOffset;
                
                yield return null;
            }

            scaleObject.transform.position = originalPosition;
        }

        private void StartContinuousShake()
        {
            if (scaleObject == null) return;
            
            isContinuousShaking = true;
            lastVibrationTime = Time.time; 
            continuousShakeCoroutine = StartCoroutine(ContinuousShakeCoroutine());
            
            #if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
            #endif
        }

        private void StopContinuousShake()
        {
            isContinuousShaking = false;
            
            if (continuousShakeCoroutine != null)
            {
                StopCoroutine(continuousShakeCoroutine);
                continuousShakeCoroutine = null;
            }
            
            if (scaleObject != null)
            {
            }
        }

        private IEnumerator ContinuousShakeCoroutine()
        {
            Vector3 originalPosition = scaleObject.transform.position;
            
            while (isContinuousShaking && timer != null && timer.CurrentTime <= lastSecondsThreshold)
            {
                if (Time.time - lastVibrationTime >= 1f)
                {
                    #if MODULE_HAPTIC
                    Haptic.Play(Haptic.HAPTIC_MEDIUM);
                    #endif
                    lastVibrationTime = Time.time;
                }
                
                float shakeX = UnityEngine.Random.Range(-continuousShakeIntensity, continuousShakeIntensity);
                float shakeY = UnityEngine.Random.Range(-continuousShakeIntensity, continuousShakeIntensity);
                
                Vector3 shakeOffset = new Vector3(shakeX, shakeY, 0f);
                scaleObject.transform.position = originalPosition + shakeOffset;
                
                yield return null;
            }
            
            if (scaleObject != null)
            {
                scaleObject.transform.position = originalPosition;
            }
            
            isContinuousShaking = false;
        }
    }
}
