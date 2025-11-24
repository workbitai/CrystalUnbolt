using System.Collections.Generic;
using UnityEngine;

namespace CrystalUnbolt
{
    public class TweenCaseCollection
    {
        private List<AnimCase> tweenCases = new List<AnimCase>();
        public List<AnimCase> TweenCases => tweenCases;

        private GameCallback tweensCompleted;

        public void AddTween(AnimCase tweenCase)
        {
            // Safety check: DelayedCall can return null if delay <= 0
            if (tweenCase == null)
            {
                Debug.LogWarning("[TweenCaseCollection] Attempted to add null tween case. This can happen when DelayedCall is called with delay <= 0.");
                return;
            }

            tweenCase.OnComplete(OnTweenCaseComplete);

            tweenCases.Add(tweenCase);
        }

        public bool IsComplete()
        {
            for(int i = 0; i < tweenCases.Count; i++)
            {
                if (!tweenCases[i].IsCompleted)
                    return false;
            }

            return true;
        }

        public void Complete()
        {
            for (int i = 0; i < tweenCases.Count; i++)
            {
                tweenCases[i].Complete();
            }
        }

        public void Kill()
        {
            for (int i = 0; i < tweenCases.Count; i++)
            {
                tweenCases[i].Kill();
            }
        }

        public void OnComplete(GameCallback callback)
        {
            tweensCompleted += callback;
        }

        private void OnTweenCaseComplete()
        {
            for (int i = 0; i < tweenCases.Count; i++)
            {
                if (!tweenCases[i].IsCompleted)
                    return;
            }

            if (tweensCompleted != null)
                tweensCompleted.Invoke();
        }

        public static TweenCaseCollection operator +(TweenCaseCollection caseCollection, AnimCase tweenCase)
        {
            if(caseCollection == null)
                caseCollection = new TweenCaseCollection();

            caseCollection.AddTween(tweenCase);

            return caseCollection;
        }
    }
}
