using UnityEngine;

namespace CrystalUnbolt
{
    public static class ParticleTweenCases
    {
        #region Extensions
        public static AnimCase WaitForEnd(this ParticleSystem tweenObject, float delay = 0, bool unscaledTime = false, UpdateMethod tweenType = UpdateMethod.Update)
        {
            return new Wait(tweenObject).SetDelay(delay).SetUnscaledMode(unscaledTime).SetUpdateMethod(tweenType).StartTween();
        }
        #endregion

        public class Wait : AnimCase
        {
            public ParticleSystem particleSystem;

            public Wait(ParticleSystem particleSystem)
            {
                this.particleSystem = particleSystem;

                duration = float.MaxValue;
            }

            public override void DefaultComplete()
            {

            }

            public override void Invoke(float deltaTime)
            {
                if (!particleSystem.IsAlive())
                    Complete();
            }

            public override bool Validate()
            {
                return particleSystem != null;
            }
        }
    }
}