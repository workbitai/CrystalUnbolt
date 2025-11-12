using System.Collections.Generic;
using UnityEngine;

namespace CrystalUnbolt
{
    public class CrystalTutorialCanvasController : MonoBehaviour
    {
        private static CrystalTutorialCanvasController instance;

        public static readonly int POINTER_DEFAULT = Animator.StringToHash("Click");
        public static readonly int POINTER_SHOW_PU = Animator.StringToHash("Show");

        [SerializeField] CanvasGroup fadeCanvasGroup;

        [Space]
        [SerializeField] Animator pointerAnimator;

        private static Canvas tutorialCanvas;

        private static List<TransformCase> activeTransformCases;

        private static AnimCase fadeTweenCase;

        public void Init()
        {
            instance = this;

            tutorialCanvas = GetComponent<Canvas>();
            tutorialCanvas.enabled = false;

            activeTransformCases = new List<TransformCase>();
        }

        public static void ActivatePointer(Vector3 position, int animationHash)
        {
            Transform pointerTransform = instance.pointerAnimator.transform;
            pointerTransform.gameObject.SetActive(true);
            pointerTransform.position = position;
            pointerTransform.localPosition = pointerTransform.localPosition.SetZ(0);
            pointerTransform.SetAsLastSibling();

            tutorialCanvas.enabled = true;

            instance.pointerAnimator.Play(animationHash, -1, 0);
        }

        public static void ActivateTutorialCanvas(RectTransform element, bool createDummy, bool fadeImage)
        {
            TransformCase activeTransformCase = new TransformCase(element);
            activeTransformCase.SetNewParent(tutorialCanvas.transform, createDummy);

            activeTransformCases.Add(activeTransformCase);

            tutorialCanvas.enabled = true;

            if (fadeImage)
            {
                if (!instance.fadeCanvasGroup.gameObject.activeSelf)
                {
                    fadeTweenCase.KillActive();

                    instance.fadeCanvasGroup.gameObject.SetActive(true);
                    instance.fadeCanvasGroup.alpha = 0;

                    fadeTweenCase = instance.fadeCanvasGroup.DOFade(1.0f, 0.3f);
                }
            }
        }

        public static void ResetTutorialCanvas()
        {
            foreach (TransformCase transformCase in activeTransformCases)
            {
                transformCase.Reset();
            }
            activeTransformCases.Clear();

            fadeTweenCase.KillActive();

            instance.fadeCanvasGroup.alpha = 0;
            instance.fadeCanvasGroup.gameObject.SetActive(false);

            instance.pointerAnimator.gameObject.SetActive(false);

            tutorialCanvas.enabled = false;
        }

        public static void ResetPointer()
        {
            instance.pointerAnimator.gameObject.SetActive(false);

            tutorialCanvas.enabled = false;
        }

        private class TransformCase
        {
            private RectTransform rectTransform;

            private Transform parentTransform;

            private Vector2 anchoredPosition;
            private Vector2 size;
            private Vector3 scale;
            private Quaternion rotation;

            private int siblingIndex;

            private GameObject dummyObject;

            private bool isActive;

            public TransformCase(RectTransform element)
            {
                rectTransform = element;

                siblingIndex = element.GetSiblingIndex();

                parentTransform = element.parent;

                anchoredPosition = element.anchoredPosition;
                size = element.sizeDelta;
                scale = element.localScale;
                rotation = element.localRotation;

                isActive = true;
            }

            public void SetNewParent(Transform transform, bool createDummy)
            {
                if (createDummy)
                {
                    dummyObject = new GameObject("[TUTORIAL DUMMY]", typeof(RectTransform));
                    dummyObject.transform.SetParent(parentTransform);
                    dummyObject.transform.SetSiblingIndex(siblingIndex);

                    RectTransform dummyRectTransform = (RectTransform)dummyObject.transform;
                    dummyRectTransform.anchoredPosition = anchoredPosition;
                    dummyRectTransform.sizeDelta = size;
                    dummyRectTransform.localScale = scale;
                    dummyRectTransform.localRotation = rotation;

                    dummyObject.SetActive(true);
                }

                rectTransform.SetParent(transform, true);
            }

            public void Reset()
            {
                if (!isActive) return;

                isActive = false;

                if (dummyObject != null)
                    Destroy(dummyObject);

                rectTransform.SetParent(parentTransform, true);
                rectTransform.anchoredPosition = anchoredPosition;
                rectTransform.sizeDelta = size;
                rectTransform.localScale = scale;
                rectTransform.localRotation = rotation;

                rectTransform.SetSiblingIndex(siblingIndex);
            }
        }
    }
}