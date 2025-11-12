using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace General
{
    public class CardSwipeController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public static CardSwipeController Inst;

        [SerializeField] private RectTransform targetRect;
        private Vector2 startPos;
        [SerializeField] private List<RectTransform> cardsTheme = new List<RectTransform>();
        [SerializeField] private List<Image> pageDots = new List<Image>();
        [SerializeField] private Sprite activeSp, defaultSp;
        private bool isSwipe = false;

        private float diffX = 710;
        private float scaleDef = 0.65f;

        public int currentTheme = 1;

        private void Awake()
        {
            if (Inst == null) Inst = this;
            SwipeCard(0);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.pointerCurrentRaycast.gameObject == null) return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), Input.mousePosition,
                Camera.main, out Vector2 localPos);
            startPos = localPos;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerDrag.gameObject == null) return;

            if (!isSwipe)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(),
                    Input.mousePosition,
                    Camera.main, out Vector2 localPos);

                if ((startPos.x - localPos.x) > 5)
                {
                    isSwipe = true;
                    SwipeCard(1);
                }

                if ((startPos.x - localPos.x) < -5)
                {
                    isSwipe = true;
                    SwipeCard(-1);
                }
            }
        }

        public void SwipeCard(int count)
        {
            currentTheme += count;
            if (currentTheme < 0)
            {
                currentTheme = 0;
                return;
            }

            if (currentTheme > 5)
            {
                currentTheme = 5;
                return;
            }

            for (int i = 0; i < cardsTheme.Count; i++)
            {
                var diffPos = -(diffX * (currentTheme - i));

                cardsTheme[i].DOAnchorPosX(diffPos, 0.3f).SetEase(Ease.Linear);
                cardsTheme[i].DOScale(Vector3.one * (i == currentTheme ? 0.85f : scaleDef), 0.3f).SetEase(Ease.Linear);
            }

            for (int i = 0; i < pageDots.Count; i++)
            {
                pageDots[i].sprite = i == currentTheme ? activeSp : defaultSp;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isSwipe = false;
        }
    }
}