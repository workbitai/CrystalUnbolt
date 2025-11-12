using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalUnbolt
{
    public static class TweenExtensions
    {
        public static bool ExistsAndActive(this AnimCase tweenCase)
        {
            return tweenCase != null && tweenCase.IsActive;
        }

        public static bool KillActive(this AnimCase tweenCase)
        {
            if (tweenCase != null && tweenCase.IsActive)
            {
                tweenCase.Kill();

                return true;
            }

            return false;
        }

        public static void KillActive(this AnimCase[] tweenCases)
        {
            if(tweenCases != null)
            {
                foreach (AnimCase tweenCase in tweenCases)
                {
                    if (tweenCase != null && tweenCase.IsActive)
                    {
                        tweenCase.Kill();
                    }
                }
            }
        }

        public static bool KillActive(this TweenCaseCollection tweenCase)
        {
            if (tweenCase != null && !tweenCase.IsComplete())
            {
                tweenCase.Kill();

                return true;
            }

            return false;
        }
        
        public static bool CompleteActive(this AnimCase tweenCase)
        {
            if (tweenCase != null && !tweenCase.IsCompleted)
            {
                tweenCase.Complete();

                return true;
            }

            return false;
        }

        public static void CompleteActive(this AnimCase[] tweenCases)
        {
            if (tweenCases != null)
            {
                foreach (AnimCase tweenCase in tweenCases)
                {
                    if (tweenCase != null && tweenCase.IsActive)
                    {
                        tweenCase.Complete();
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnimCase DoSnapTargetBottom(this ScrollRect scrollRect, RectTransform target, float duration, float offsetX = 0, float offsetY = 0)
        {
            var targetPosition = target.position + Vector3.up * (scrollRect.viewport.rect.height / 2 + target.sizeDelta.y);
            return scrollRect.SnapToTarget(targetPosition, duration, offsetX, offsetY);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnimCase DoSnapTargetTop(this ScrollRect scrollRect, RectTransform target, float duration, float offsetX = 0, float offsetY = 0)
        {
            return scrollRect.SnapToTarget(target.position, duration, offsetX, offsetY);
        }

        public static AnimCase SnapToTarget(this ScrollRect scrollRect, Vector3 target, float duration, float offsetX = 0, float offsetY = 0)
        {
            Vector2 contentPosition = scrollRect.viewport.InverseTransformPoint(scrollRect.content.position);
            Vector2 newPosition = scrollRect.viewport.InverseTransformPoint(target);
            newPosition = new Vector2(newPosition.x + offsetX, newPosition.y + offsetY);

            if (!scrollRect.horizontal)
                newPosition.x = contentPosition.x;

            if (!scrollRect.vertical)
                newPosition.y = contentPosition.y;

            return scrollRect.content.DOAnchoredPosition(contentPosition - newPosition, duration);
        }
    }
}