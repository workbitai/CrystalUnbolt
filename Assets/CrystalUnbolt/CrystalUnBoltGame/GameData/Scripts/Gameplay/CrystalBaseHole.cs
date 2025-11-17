using UnityEngine;

namespace CrystalUnbolt
{
    public class CrystalBaseHole : CrystalHoleController
    {
        [SerializeField] CircleCollider2D circleCollider;
        [Header("Locked Hole Visuals")]
        [SerializeField] private Sprite unlockIconSprite;
        [SerializeField] private Vector3 unlockIconOffset = new Vector3(0.35f, 0.35f, 0f);
        [SerializeField] private float unlockIconScale = 1f;
        [SerializeField] private float unlockIconClickRadius = 0.32f;

        public float ColliderRadius => circleCollider.radius;
        public float PhysicsRadius => circleCollider.radius * CrystalGameManager.Data.HoleVisibleAmountToEnableScrew;

        public event SimpleBoolCallback StateChanged;
        public SpriteRenderer hole;
        [SerializeField] private SpriteRenderer rippleRenderer;

        private bool isLocked;
        private bool unlockRequestInProgress;
        private CrystalLockedHoleConfig activeLockConfig;
        private CrystalHoleUnlockOverlay unlockOverlay;

        public bool IsLocked => isLocked;

        public void Init(CrystalHoleData data)
        {
            StateChanged = null;

            transform.position = data.Position.SetZ(0.9f);

            IsActive = false;
            activeLockConfig = null;
            SetLockedState(false, true);

            EnsureRippleRenderer();
            if (rippleRenderer != null)
            {
                rippleRenderer.gameObject.SetActive(false);
                rippleRenderer.color = new Color(1f, 1f, 1f, 0f);
                rippleRenderer.transform.localScale = Vector3.one;
            }
        }

        public void SetActive(bool isActive)
        {
            IsActive = isActive;

            StateChanged?.Invoke(isActive);
        }

        public override void Discard()
        {
            IsActive = false;
            gameObject.SetActive(false);

            StateChanged?.Invoke(false);
        }
        public void ResetDataHole(bool preserveLock = false)
        {
            if (!preserveLock)
            {
                ClearLock();
            }
            else if (isLocked)
            {
                SetLockedState(true);
            }

            gameObject.transform.localScale = Vector3.one;
            if (rippleRenderer != null)
            {
                rippleRenderer.transform.localScale = Vector3.one;
                rippleRenderer.color = new Color(1f, 1f, 1f, 0f);
                rippleRenderer.gameObject.SetActive(false);
            }
        }

        public void ApplyLock(CrystalLockedHoleConfig config)
        {
            if (config == null)
                return;
            Debug.Log("Apply Lock 0");
            activeLockConfig = config;
            SetLockedState(true);
        }

        public void ClearLock()
        {
            activeLockConfig = null;
            unlockRequestInProgress = false;
            SetLockedState(false);
        }

        internal void HandleUnlockRequest()
        {
            if (!isLocked || unlockRequestInProgress)
                return;

            unlockRequestInProgress = true;
            unlockOverlay?.SetBusy(true);

            switch (activeLockConfig?.UnlockType)
            {
                case CrystalHoleUnlockType.None:
                    CompleteUnlock();
                    break;
                case CrystalHoleUnlockType.RewardedAd:
                default:
                    RequestRewardedVideo();
                    break;
            }
        }

        private void RequestRewardedVideo()
        {
            MyAdsAdapter.ShowRewardBasedVideo((bool success) =>
            {
                unlockRequestInProgress = false;

                if (success)
                {
                    CompleteUnlock();
                }
                else
                {
                    unlockOverlay?.SetBusy(false);
                }
            });
        }

        private void CompleteUnlock()
        {
            unlockOverlay?.SetBusy(false);
            ClearLock();
            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
        }

        private void SetLockedState(bool locked, bool forceReset = false)
        {
            isLocked = locked;
            circleCollider.enabled = !locked;

            if (!locked || forceReset)
            {
                unlockOverlay?.SetActive(false);
                return;
            }

            EnsureUnlockOverlay();
            unlockOverlay?.ApplyConfig(activeLockConfig, unlockIconOffset, unlockIconScale, unlockIconClickRadius);
            unlockOverlay?.SetActive(true);
        }

        private void EnsureUnlockOverlay()
        {
            if (unlockOverlay != null || unlockIconSprite == null)
            {
                if (unlockOverlay == null && unlockIconSprite == null)
                {
                    Debug.LogWarning("[CrystalBaseHole] Unlock icon sprite is not assigned. Locked hole icon will not be shown.");
                }
                return;
            }

            unlockOverlay = CrystalHoleUnlockOverlay.Create(this, unlockIconSprite, unlockIconOffset, unlockIconScale, unlockIconClickRadius);
        }

        public SpriteRenderer GetOrCreateRippleRenderer()
        {
            EnsureRippleRenderer();
            return rippleRenderer;
        }

        private void EnsureRippleRenderer()
        {
            if (rippleRenderer != null || hole == null)
                return;

            GameObject rippleObject = new GameObject($"{hole.name}_Ripple");
            rippleObject.transform.SetParent(hole.transform, false);
            rippleObject.transform.localPosition = Vector3.zero;
            rippleObject.transform.localRotation = Quaternion.identity;

            rippleRenderer = rippleObject.AddComponent<SpriteRenderer>();
            rippleRenderer.sprite = hole.sprite;
            rippleRenderer.color = new Color(1f, 1f, 1f, 0f);
            rippleRenderer.sortingLayerID = hole.sortingLayerID;
            rippleRenderer.sortingOrder = hole.sortingOrder + 1;
        }
    }
}
