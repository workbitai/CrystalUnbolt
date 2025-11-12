using UnityEngine;

namespace CrystalUnbolt
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class CrystalBaseController : MonoBehaviour, IClickableObject
    {
        public Vector3 Position => transform.position;

        public BoxCollider2D BoxCollider { get; private set; }

        private void Awake()
        {
            BoxCollider = GetComponent<BoxCollider2D>();    
        }

        public void OnObjectClicked(Vector3 clickPosition)
        {

        }

        public void Discard()
        {
            gameObject.SetActive(false);
        }
    }
}