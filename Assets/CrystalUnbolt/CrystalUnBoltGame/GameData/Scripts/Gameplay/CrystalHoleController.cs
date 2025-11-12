using UnityEngine;

namespace CrystalUnbolt
{
    public abstract class CrystalHoleController : MonoBehaviour, IClickableObject
    {
        public Vector3 Position => transform.position;

        public bool IsActive { get; protected set; }
        private int placedNumber = -1;
        public int PlacedNumber => placedNumber; // ? getter
        public bool IsPuzzleHole { get; set; } = false;

        public void SetPlacedNumber(int number)
        {
            placedNumber = number;
            Debug.Log($"[Hole] {name} set placedNumber = {placedNumber}");
        }
        public void ClearPlacedNumber()
        {
            placedNumber = -1;
            Debug.Log($"[Hole] {gameObject.name} cleared ? -1");
        }
        public void ResetNewData()
        {
            placedNumber = -1;
        }
        private void Awake()
        {
            IsActive = false;
        }

        public virtual void OnObjectClicked(Vector3 clickPosition)
        {

        }

        public virtual void Discard()
        {

        }
    }
}