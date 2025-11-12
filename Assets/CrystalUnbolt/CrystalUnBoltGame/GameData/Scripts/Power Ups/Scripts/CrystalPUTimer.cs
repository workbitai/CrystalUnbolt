using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrystalUnbolt
{
    public class CrystalPUTimer
    {
        public float State => (Time.time - startTime) / duration;
        public string Seconds => (duration - (Time.time - startTime)).ToString("F0");

        private float duration;
        private float startTime;

        private AnimCase delayTweenCase;

        private bool isActive;
        public bool IsActive => isActive;

        public CrystalPUTimer(float duration, GameCallback onCompleted)
        {
            this.duration = duration;

            startTime = Time.time;

            delayTweenCase = Tween.DelayedCall(duration, onCompleted);

            isActive = true;
        }

        public void Disable()
        {
            isActive = false;

            delayTweenCase.KillActive();
        }
    }
}
