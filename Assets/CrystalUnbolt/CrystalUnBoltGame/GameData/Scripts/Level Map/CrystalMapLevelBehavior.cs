using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using DGEase = DG.Tweening.Ease;

namespace CrystalUnbolt.Map
{
    public class CrystalMapLevelBehavior : CrystalMapLevelAbstractBehavior
    {
        [SerializeField] Image innerCircle;

        [Space]
        [SerializeField] Color reachedText;
        [SerializeField] Color reachedCircle;
        [Space]
        [SerializeField] Color openedText;
        [SerializeField] Color openedCircle;
        [Space]
        [SerializeField] Color closedText;
        [SerializeField] Color closedCircle;
        
        [Header("Jelly Animation")]
        [SerializeField] private bool enableJellyAnimation = true;
        [SerializeField] private bool isInGrid = false; // Set true when used in grid to prevent auto-start
        private CrystalJellyLevelAnimation jellyAnimation;
        
        [Header("Locked Level Shake")]
        [SerializeField] private float shakeStrength = 0.3f;
        [SerializeField] private float shakeDuration = 0.5f;
        private Vector3 originalPosition;

        protected override void InitOpen()
        {
            levelNumber.color = openedText;
            innerCircle.color = openedCircle;

            button.gameObject.SetActive(true);
        }

        protected override void InitClose() 
        {
            levelNumber.color = closedText;
            innerCircle.color = closedCircle;

            button.gameObject.SetActive(true); 
        }

        protected override void InitCurrent()
        {
            levelNumber.color = reachedText;
            innerCircle.color = reachedCircle;

            button.gameObject.SetActive(true);
            
            // Only auto-start jelly animation if NOT in grid mode
            if (enableJellyAnimation && !isInGrid)
            {
                StartJellyAnimation();
            }
        }
        
        public void StartJellyAnimation()
        {
            jellyAnimation = GetComponent<CrystalJellyLevelAnimation>();
            if (jellyAnimation == null)
            {
                jellyAnimation = gameObject.AddComponent<CrystalJellyLevelAnimation>();
            }
            
            jellyAnimation.StartAnimation();
        }
        
        private void StopJellyAnimation()
        {
            if (jellyAnimation != null)
            {
                jellyAnimation.StopAnimation();
            }
        }
        
        protected override void OnButtonClicked()
        {
            if (LevelId > CrystalMapBehavior.MaxLevelReached)
            {
                PlayLockedLevelShake();
                return;
            }
            
            base.OnButtonClicked();
        }
        
        private void PlayLockedLevelShake()
        {
            if (originalPosition == Vector3.zero)
            {
                originalPosition = transform.localPosition;
            }
            
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif
            
            transform.DOShakePosition(shakeDuration, shakeStrength, 10, 90, false, true)
                .SetEase(DGEase.OutQuart)
                .OnComplete(() => {
                    transform.localPosition = originalPosition;
                });
        }

        private void OnDestroy()
        {
            StopJellyAnimation();
        }
    }
}