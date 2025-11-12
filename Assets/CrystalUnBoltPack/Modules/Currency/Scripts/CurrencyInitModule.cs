using UnityEngine;

namespace CrystalUnbolt
{
    [RegisterModule("Currencies", false, order: 100)]
    public class CurrencyInitModule : GameModule
    {
        public override string ModuleName => "Currencies";

        [SerializeField] CurrencyDatabase currenciesDatabase;
        public CurrencyDatabase Database => currenciesDatabase;

        public override void CreateComponent()
        {
            Debug.Log("[CurrencyInitModule] Initializing EconomyManager");
            EconomyManager.Init(currenciesDatabase);
            Debug.Log("[CurrencyInitModule] EconomyManager initialized successfully");
        }
    }
}
