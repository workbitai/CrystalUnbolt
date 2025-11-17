using UnityEngine;

namespace CrystalUnbolt
{
    public class CrystalHoleUnlockOverlay : MonoBehaviour, IClickableObject
    {
        private SpriteRenderer iconRenderer;
        private CircleCollider2D clickCollider;
        private CrystalBaseHole owner;
        private bool interactable = true;

        public Vector3 Position => transform.position;

        public static CrystalHoleUnlockOverlay Create(CrystalBaseHole owner, Sprite sprite, Vector3 defaultOffset, float defaultScale, float defaultRadius)
        {
            GameObject go = new GameObject($"{owner.name}_UnlockOverlay");
            go.layer = owner.gameObject.layer;

            var overlay = go.AddComponent<CrystalHoleUnlockOverlay>();
            overlay.owner = owner;

            overlay.iconRenderer = go.AddComponent<SpriteRenderer>();
            overlay.iconRenderer.sprite = sprite;
            overlay.iconRenderer.sortingLayerID = owner.hole.sortingLayerID;
            overlay.iconRenderer.sortingOrder = owner.hole.sortingOrder + 5;

            overlay.clickCollider = go.AddComponent<CircleCollider2D>();
            overlay.clickCollider.isTrigger = true;
            overlay.clickCollider.radius = defaultRadius;

            overlay.transform.SetParent(owner.transform);
            overlay.transform.localRotation = Quaternion.identity;
            overlay.transform.localScale = Vector3.one * defaultScale;
            overlay.transform.localPosition = defaultOffset;

            overlay.SetActive(false);
            return overlay;
        }

        public void ApplyConfig(CrystalLockedHoleConfig config, Vector3 defaultOffset, float defaultScale, float defaultRadius)
        {
            Vector3 offset = defaultOffset;
            float scale = defaultScale;
            float radius = defaultRadius;

            if (config != null)
            {
                offset = new Vector3(config.IconOffset.x, config.IconOffset.y, defaultOffset.z);
                scale = config.IconScale > 0 ? config.IconScale : defaultScale;
                radius = config.ClickRadius > 0 ? config.ClickRadius : defaultRadius;
            }

            transform.localPosition = offset;
            transform.localScale = Vector3.one * scale;

            if (clickCollider != null)
            {
                clickCollider.radius = radius;
            }
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
            interactable = active;
        }

        public void SetBusy(bool busy)
        {
            interactable = !busy;
            if (iconRenderer != null)
            {
                iconRenderer.color = busy ? new Color(1f, 1f, 1f, 0.5f) : Color.white;
            }
        }

        public void OnObjectClicked(Vector3 clickPosition)
        {
            if (!interactable)
                return;

            owner?.HandleUnlockRequest();
        }
    }
}

