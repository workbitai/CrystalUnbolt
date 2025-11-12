using UnityEngine;

namespace CrystalUnbolt
{
    public interface IClickableObject
    {
        public void OnObjectClicked(Vector3 clickPosition);

        public Vector3 Position { get; }
    }
}