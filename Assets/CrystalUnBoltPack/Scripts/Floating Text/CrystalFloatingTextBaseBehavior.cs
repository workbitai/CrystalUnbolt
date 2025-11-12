using TMPro;
using UnityEngine;

namespace CrystalUnbolt
{
    public abstract class CrystalFloatingTextBaseBehavior : MonoBehaviour
    {
        [SerializeField] protected TMP_Text textRef;

        public GameCallback OnAnimationCompleted;

        public virtual void Activate(string text, float scaleMultiplier, Color color)
        {
            textRef.text = text;
            textRef.color = color;

            InvokeCompleteEvent();
        }

        protected void InvokeCompleteEvent()
        {
            OnAnimationCompleted?.Invoke();
        }
    }
}