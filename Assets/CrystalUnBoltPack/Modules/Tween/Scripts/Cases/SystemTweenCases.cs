using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrystalUnbolt
{
    public static class SystemTweenCases
    {
        #region Extensions
        public static AnimCase DOAction<T>(this object tweenObject, System.Action<T, T, float> action, T startValue, T resultValue, float time, float delay = 0, bool unscaledTime = false, UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return new Action<T>(startValue, resultValue, action).SetDelay(delay).SetDuration(time).SetUnscaledMode(unscaledTime).SetUpdateMethod(updateMethod).StartTween();
        }

        public static AnimCase OnCompleted(this AsyncOperation tweenObject, GameCallback onCompleted)
        {
            return new AsyncOperationTweenCase(tweenObject).SetUnscaledMode(true).SetUpdateMethod(UpdateMethod.Update).OnComplete(onCompleted).StartTween();
        }
        #endregion

        public class Default : AnimCase
        {
            public override void DefaultComplete() { }
            public override void Invoke(float deltaTime) { }

            public override bool Validate()
            {
                return true;
            }
        }

        public class Condition : AnimCase
        {
            public TweenConditionCallback callback;

            public Condition(TweenConditionCallback callback)
            {
                this.callback = callback;
            }

            public override void DefaultComplete()
            {

            }

            public override void Invoke(float deltaTime)
            {
                callback.Invoke(this);
            }

            public override bool Validate()
            {
                return true;
            }

            public delegate void TweenConditionCallback(Condition tweenCase);
        }

        public class Action<T> : AnimCase
        {
            private System.Action<T, T, float> action;

            private T startValue;
            private T resultValue;

            public Action(T startValue, T resultValue, System.Action<T, T, float> action)
            {
                this.startValue = startValue;
                this.resultValue = resultValue;

                this.action = action;
            }

            public override bool Validate()
            {
                return true;
            }

            public override void DefaultComplete()
            {
                action.Invoke(startValue, resultValue, 1);
            }

            public override void Invoke(float deltaTime)
            {
                action.Invoke(startValue, resultValue, Interpolate(state));
            }
        }

        public class NextFrame : AnimCase
        {
            private GameCallback callback;
            protected int framesOffset;

            public NextFrame(GameCallback callback, int framesOffset)
            {
                this.callback = callback;
                this.framesOffset = framesOffset + 1;
            }

            public override void Invoke(float deltaTime)
            {
                framesOffset--;

                if (framesOffset <= 0)
                    Complete();
            }

            public override void DefaultComplete()
            {
                callback.Invoke();
            }

            public override bool Validate()
            {
                return true;
            }
        }

        public class Float : AnimCase
        {
            public float startValue;
            public float resultValue;

            public TweenFloatCallback callback;

            public Float(float startValue, float resultValue, TweenFloatCallback callback)
            {
                this.startValue = startValue;
                this.resultValue = resultValue;

                this.callback = callback;
            }

            public override bool Validate()
            {
                return true;
            }

            public override void DefaultComplete()
            {
                callback.Invoke(resultValue);
            }

            public override void Invoke(float deltaTime)
            {
                callback.Invoke(Mathf.LerpUnclamped(startValue, resultValue, Interpolate(state)));
            }

            public delegate void TweenFloatCallback(float value);
        }

        public class ColorCase : AnimCase
        {
            public Color startValue;
            public Color resultValue;

            public TweenColorCallback callback;

            public ColorCase(Color startValue, Color resultValue, TweenColorCallback callback)
            {
                this.startValue = startValue;
                this.resultValue = resultValue;

                this.callback = callback;
            }

            public override bool Validate()
            {
                return true;
            }

            public override void DefaultComplete()
            {
                callback.Invoke(resultValue);
            }

            public override void Invoke(float deltaTime)
            {
                callback.Invoke(Color.LerpUnclamped(startValue, resultValue, Interpolate(state)));
            }

            public delegate void TweenColorCallback(Color color);
        }

        public class AsyncOperationTweenCase : AnimCase
        {
            public AsyncOperation asyncOperation;

            public float Progress => asyncOperation.progress;
            public bool IsOperationDone => asyncOperation.isDone;

            public AsyncOperationTweenCase(AsyncOperation asyncOperation)
            {
                this.asyncOperation = asyncOperation;

                duration = float.MaxValue;
            }

            public override void DefaultComplete()
            {

            }

            public override void Invoke(float deltaTime)
            {
                if (asyncOperation.progress >= 1.0f)
                    Complete();
            }

            public override bool Validate()
            {
                return true;
            }
        }
    }
}
