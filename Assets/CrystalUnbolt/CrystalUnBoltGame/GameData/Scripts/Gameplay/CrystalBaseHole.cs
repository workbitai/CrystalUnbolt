using System.Drawing;
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

        public void Init(CrystalHoleData data)
        {
            StateChanged = null;

            transform.position = data.Position.SetZ(0.9f);

            IsActive = false;
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
        
        }
    }
}
