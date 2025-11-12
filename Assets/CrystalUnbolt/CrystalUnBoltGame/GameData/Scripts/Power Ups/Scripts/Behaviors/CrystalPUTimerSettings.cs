using UnityEngine;

namespace CrystalUnbolt
{
    [CreateAssetMenu(fileName = "PU Timer Settings", menuName = "Content/Power Ups/PU Timer Settings")]
    public class CrystalPUTimerSettings : CrystalPUSettings
    {
        [LineSpacer("Timer")]
        [SerializeField] float timeFreezeDuration = 10.0f;
        public float TimeFreezeDuration => timeFreezeDuration;

        public override void Init()
        {

        }
    }
}
