#pragma warning disable 0618

using UnityEngine;
using UnityEngine.UI;

namespace CrystalUnbolt
{
    [System.Serializable]
    public class CrystalUIMainMenuButton
    {
      //  [SerializeField] RectTransform rect;
        [SerializeField] Button button;
        public Button Button => button;

        private float savedRectPosX;
        private float rectXPosBehindOfTheScreen;

        public void Init(float rectXPosBehindOfTheScreen)
        {
            this.rectXPosBehindOfTheScreen = rectXPosBehindOfTheScreen;
           // savedRectPosX = rect.anchoredPosition.x;
        }

        public void Show(bool immediately = false)
        {
            SetPosition(savedRectPosX);
           // rect.localScale = Vector3.one;
        }

        public void Hide(bool immediately = false)
        {
            SetPosition(rectXPosBehindOfTheScreen);
        }

        public void HideWithPop(bool immediately = false)
        {
           // rect.localScale = Vector3.zero;
        }

        private void SetPosition(float xPos)
        {
          //  Vector2 pos = rect.anchoredPosition;
        //    pos.x = xPos;
          //  rect.anchoredPosition = pos;
        }







    }
}
