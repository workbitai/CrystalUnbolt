using UnityEngine;
using UnityEngine.UI;

namespace CrystalUnbolt
{
    [RequireComponent(typeof(Canvas)), RequireComponent(typeof(GraphicRaycaster))]
    public abstract class BaseScreen : MonoBehaviour
    {
        protected bool isPageDisplayed;
        public bool IsPageDisplayed { get => isPageDisplayed; set => isPageDisplayed = value; }

        protected Canvas canvas;
        public Canvas Canvas => canvas;

        protected GraphicRaycaster graphicRaycaster;
        public GraphicRaycaster GraphicRaycaster => graphicRaycaster;

        private string defaultName;

        public void CacheComponents()
        {
            defaultName = name;

            canvas = GetComponent<Canvas>();
            graphicRaycaster = GetComponent<GraphicRaycaster>();
        }

        public abstract void Init();

        public void EnableCanvas()
        {
            Debug.Log($"[UIPage] EnableCanvas called on {GetType().Name}");
            isPageDisplayed = true;

            canvas.enabled = true;

#if UNITY_EDITOR
            name = string.Format("{0} (Active)", defaultName);
#endif
        }

        public void DisableCanvas()
        {
            Debug.Log($"[UIPage] DisableCanvas called on {GetType().Name}");
            isPageDisplayed = false;

            canvas.enabled = false;

#if UNITY_EDITOR
            name = defaultName;
#endif
        }

        public abstract void PlayShowAnimation();
        public abstract void PlayHideAnimation();

        public abstract void PlayShowAnimationMainReturn();
        public virtual void Unload()
        {
            isPageDisplayed = false;

            canvas.enabled = false;
        }
    }
}