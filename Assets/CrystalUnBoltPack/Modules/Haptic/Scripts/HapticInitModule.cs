using UnityEngine;

namespace CrystalUnbolt
{
    [RegisterModule("Haptic")]
    public class HapticInitModule : GameModule
    {
        [SerializeField] bool verboseLogging = false;

        public override string ModuleName => "Haptic";

        public override void CreateComponent()
        {
            if (verboseLogging)
                Haptic.EnableVerboseLogging();

            Haptic.Init();
        }
    }
}
