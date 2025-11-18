using UnityEngine;
using DG.Tweening;
using System.Collections;

namespace CrystalUnbolt
{
    public class CrystalPUExtraHoleBehavior : CrystalPUBehavior
    {
        [SerializeField] Particle particle;
        
        [Header("Drill Animation")]
        [SerializeField] GameObject drillPrefab; 
        [SerializeField] Transform parentObject; 
        [SerializeField] bool useLeftCornerAlignment = true; 
        [SerializeField] float drillAnimationDuration = 1.0f;
        [SerializeField] float drillRotationSpeed = 360f;
        [SerializeField] float drillScaleUp = 1.2f;
        
        [Header("Haptics")]
        [SerializeField] bool useHaptics = true;
        [SerializeField] bool vibrateDuringDrill = true; 
        [SerializeField] bool vibrateOnImpact = true; 
        [SerializeField, Range(0f, 1f)] float drillHapticStrength = 0.6f; 
        [SerializeField, Range(0f, 1f)] float impactHapticStrength = 0.8f; 
        
        private AnimCase drillMoveCase;
        private AnimCase drillRotateCase;
        private AnimCase drillScaleCase;

        public override void Init()
        {
            CrystalParticlesController.RegisterParticle(particle);
        }

        public override bool Activate()
        {
            return true;
        }

        public override void OnSelected()
        {
            base.OnSelected();

            if (CrystalScrewController.SelectedScrew != null) CrystalScrewController.SelectedScrew.Deselect();
        }

        public override bool ApplyToElement(IClickableObject clickableObject, Vector3 clickPosition)
        {
            if (PowerUpLock.IsLocked) return false; 

            if (clickableObject is CrystalBaseController baseBehavior)
            {
                Collider2D[] colliders2D = Physics2D.OverlapCircleAll(clickPosition, 0.4f);
                bool pointIsAllowed = true;
                for (int i = 0; i < colliders2D.Length; i++)
                {
                    Collider2D collider = colliders2D[i];
                    if (collider == null)
                        continue;

                    if (collider == baseBehavior.BoxCollider)
                        continue;

                    if (collider.GetComponentInParent<CrystalBaseController>() == baseBehavior)
                        continue;

                    if (collider.GetComponent<CrystalBaseHole>() != null)
                        continue;

                    if (collider.isTrigger)
                        continue;

                    pointIsAllowed = false;
                    break;
                }

                if (pointIsAllowed)
                {
                    StartCoroutine(LockDuringDrillAnimation(clickPosition)); 
                    return true;
                }
            }

            return false;
        }

        IEnumerator LockDuringDrillAnimation(Vector3 targetPos)
        {
            PowerUpLock.IsLocked = true; 

            StartDrillAnimation(targetPos);

            yield return new WaitForSeconds(drillAnimationDuration + 0.5f);

            PowerUpLock.IsLocked = false; 
        }

        private void StartDrillAnimation(Vector3 targetPosition)
        {
            if (drillPrefab == null)
            {
                particle.Play().SetPosition(targetPosition.SetZ(0.9f));
                CrystalLevelController.PlaceAdditionalBaseHole(targetPosition);
                return;
            }

            drillMoveCase.KillActive();
            drillRotateCase.KillActive();
            drillScaleCase.KillActive();

            Vector3 startPosition = GetPowerUpButtonPosition();
            
            GameObject drillInstance = Instantiate(drillPrefab, startPosition, Quaternion.identity);
            drillInstance.name = "DrillAnimation";
            
            Transform targetParent = parentObject;
            
            if (targetParent == null)
            {
                CrystalUIGame uiGame = ScreenManager.GetPage<CrystalUIGame>();
                if (uiGame != null)
                {
                    targetParent = uiGame.transform; 
                }
            }
            
            if (targetParent == null)
            {
                targetParent = transform;
            }
            
            drillInstance.transform.SetParent(targetParent, true);
            
            drillInstance.transform.localScale = Vector3.one;
            drillInstance.transform.rotation = Quaternion.identity;
            
            StartCoroutine(RealDrillAnimationSequence(drillInstance, targetPosition));
        }

        private System.Collections.IEnumerator RealDrillAnimationSequence(GameObject drillInstance, Vector3 targetPosition)
        {
            drillInstance.transform.localScale = Vector3.one * 0.5f;
            
            drillInstance.transform.position = targetPosition;
            
            yield return new WaitForSeconds(0.1f);
            
            StartCoroutine(DrillVibrationEffect(drillInstance, 0.5f));
            
            if (useHaptics && vibrateDuringDrill)
            {
                StartCoroutine(ContinuousDrillHaptics(0.5f));
            }
            
            yield return new WaitForSeconds(0.5f);
            
            drillInstance.transform.DOKill();
            
            if (useHaptics && vibrateOnImpact)
            {
#if MODULE_HAPTIC
                Haptic.Play(Haptic.HAPTIC_MEDIUM);
#else
                // Fallback vibration for platforms without haptic module
                TriggerFallbackVibration(impactHapticStrength, 0.1f);
#endif
            }
            
            particle.Play().SetPosition(targetPosition.SetZ(0.9f));
            CrystalLevelController.PlaceAdditionalBaseHole(targetPosition);
            
            
            SpriteRenderer drillRenderer = drillInstance.GetComponent<SpriteRenderer>();
            if (drillRenderer != null)
            {
                Color originalColor = drillRenderer.color;
                Color fadeColor = originalColor;
                fadeColor.a = 0f;
                drillRenderer.DOColor(fadeColor, 0.3f).SetEasing(Ease.Type.CircIn);
            }
            
            yield return new WaitForSeconds(0.3f);
            
            if (drillInstance != null)
                Destroy(drillInstance);
        }

        private System.Collections.IEnumerator DrillVibrationEffect(GameObject drillInstance, float duration)
        {
            float elapsed = 0f;
            Vector3 originalPosition = drillInstance.transform.position;
            Camera mainCamera = Camera.main;
            Vector3 originalCameraPosition = mainCamera.transform.position;
            
            while (elapsed < duration)
            {
                Vector3 vibration = new Vector3(
                    Random.Range(-0.02f, 0.02f),
                    Random.Range(-0.02f, 0.02f),
                    0f
                );
                
                drillInstance.transform.position = originalPosition + vibration;
                
                Vector3 cameraShake = new Vector3(
                    Random.Range(-0.05f, 0.05f),
                    Random.Range(-0.05f, 0.05f),
                    0f
                );
                
                mainCamera.transform.position = originalCameraPosition + cameraShake;
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            drillInstance.transform.position = originalPosition;
            mainCamera.transform.position = originalCameraPosition;
        }

        private System.Collections.IEnumerator ContinuousDrillHaptics(float duration)
        {
            float elapsed = 0f;
            float hapticInterval = 0.1f; 
            float nextHapticTime = 0f;
            
            while (elapsed < duration)
            {
                if (elapsed >= nextHapticTime)
                {
#if MODULE_HAPTIC
                    if (drillHapticStrength >= 0.8f)
                        Haptic.Play(Haptic.HAPTIC_HARD);
                    else if (drillHapticStrength >= 0.4f)
                        Haptic.Play(Haptic.HAPTIC_HARD);
                    else
                        Haptic.Play(Haptic.HAPTIC_HARD);
#else
                    // Fallback vibration for platforms without haptic module
                    TriggerFallbackVibration(drillHapticStrength, 0.05f);
#endif
                    nextHapticTime += hapticInterval;
                }
                
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        private void TriggerFallbackVibration(float strength01, float durationSec)
        {
            CrystalUniversalVibration.Vibrate(durationSec, strength01);
        }

        private Vector3 GetPowerUpButtonPosition()
        {
            CrystalUIGame uiGame = ScreenManager.GetPage<CrystalUIGame>();
            if (uiGame != null && uiGame.PowerUpsUIController != null)
            {
                CrystalPUUIBehavior powerUpPanel = uiGame.PowerUpsUIController.GetPanel(CrystalPUType.AddExtraHole);
                if (powerUpPanel != null)
                {
                    return powerUpPanel.transform.position;
                }
            }
            
            return Camera.main.ScreenToWorldPoint(new Vector3(Screen.width * 0.2f, Screen.height * 0.1f, 10f));
        }

        public override bool IsSelectable() => true;
    }
}