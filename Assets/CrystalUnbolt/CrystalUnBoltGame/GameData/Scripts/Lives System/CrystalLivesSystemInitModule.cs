using UnityEngine;

namespace CrystalUnbolt
{
    [RegisterModule("Lives System", core: false)]
    public class CrystalLivesSystemInitModule : GameModule
    {
        public override string ModuleName => "Lives System";

        [SerializeField] CrystalLivesData livesData;

        public override void CreateComponent()
        {
            if (livesData == null)
            {
                Debug.LogError("CrystalLivesData is not assigned in Project Init Settings", this);

                return;
            }

            CrystalLivesSystem.Init(livesData);
        }
    }
}