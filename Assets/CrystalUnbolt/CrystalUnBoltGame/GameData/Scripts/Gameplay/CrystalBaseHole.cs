using UnityEngine;

namespace CrystalUnbolt
{
    public class CrystalBaseHole : CrystalHoleController
    {
        [SerializeField] CircleCollider2D circleCollider;

        public float ColliderRadius => circleCollider.radius;
        public float PhysicsRadius => circleCollider.radius * CrystalGameManager.Data.HoleVisibleAmountToEnableScrew;

        public event SimpleBoolCallback StateChanged;
        public SpriteRenderer hole;
        [SerializeField] private SpriteRenderer rippleRenderer;

        public void Init(CrystalHoleData data)
        {
            StateChanged = null;

            transform.position = data.Position.SetZ(0.9f);

            IsActive = false;

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
        public void ResetDataHole()
        {
           
            gameObject.transform.localScale = Vector3.one;
            if (rippleRenderer != null)
            {
                rippleRenderer.transform.localScale = Vector3.one;
                rippleRenderer.color = new Color(1f, 1f, 1f, 0f);
                rippleRenderer.gameObject.SetActive(false);
            }
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
