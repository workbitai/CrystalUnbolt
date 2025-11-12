#pragma warning disable 0618

using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalUnbolt
{
    [System.Serializable]
    public class CrystalUIMainMenuButton
    {
        [SerializeField] RectTransform rect;
        [SerializeField] Button button;
        public Button Button => button;

        [Space]
        [SerializeField] AnimationCurve showStoreAdButtonsCurve;
        [SerializeField] AnimationCurve hideStoreAdButtonsCurve;
        [SerializeField] float showHideDuration;

        private float savedRectPosX;
        private float rectXPosBehindOfTheScreen;

        private AnimCase showHideCase;

        public void Init(float rectXPosBehindOfTheScreen)
        {
            this.rectXPosBehindOfTheScreen = rectXPosBehindOfTheScreen;
            savedRectPosX = rect.anchoredPosition.x;
        }

        public void Show(bool immediately = false)
        {
            if (showHideCase != null && showHideCase.IsActive) return;

            if (immediately)
            {
                rect.anchoredPosition = rect.anchoredPosition.SetX(savedRectPosX);
                return;
            }

            //RESET
            rect.anchoredPosition = rect.anchoredPosition.SetX(rectXPosBehindOfTheScreen);

            showHideCase = rect.DOAnchoredPosition(rect.anchoredPosition.SetX(savedRectPosX), showHideDuration).SetCurveEasing(showStoreAdButtonsCurve);
        }

        public void Hide(bool immediately = false)
        {
            if (showHideCase != null && showHideCase.IsActive) return;

            if (immediately)
            {
                rect.anchoredPosition = rect.anchoredPosition.SetX(rectXPosBehindOfTheScreen);
                return;
            }

            //RESET
            rect.anchoredPosition = rect.anchoredPosition.SetX(savedRectPosX);

            showHideCase = rect.DOAnchoredPosition(rect.anchoredPosition.SetX(rectXPosBehindOfTheScreen), showHideDuration).SetCurveEasing(hideStoreAdButtonsCurve);
        }

        public void HideWithPop(bool immediately = false)
        {
            if (showHideCase != null && showHideCase.IsActive)
                showHideCase.Kill();

            if (immediately)
            {
                rect.localScale = Vector3.zero;
                return;
            }

            // ???? 1 ? 0 ?? smooth shrink
            showHideCase = rect.DOScale(Vector3.zero, 0.3f);
        }







    }
}
