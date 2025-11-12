using UnityEngine;

namespace CrystalUnbolt
{
    public abstract class CrystalPUSettings : ScriptableObject
    {
        [Order(-1)]
        [SerializeField] CrystalPUType type;
        public CrystalPUType Type => type;

        [Group("Refs")]
        [Space]
        [SerializeField] Sprite icon;
        public Sprite Icon => icon;

        [Group("Refs")]
        [SerializeField] GameObject behaviorPrefab;
        public GameObject BehaviorPrefab => behaviorPrefab;

        [Group("Refs")]
        [SerializeField] AudioClipToggle customAudioClip;
        public AudioClipToggle CustomAudioClip => customAudioClip;

        [Group("Variables")]
        [SerializeField] int defaultAmount;
        public int DefaultAmount => defaultAmount;

        [Group("Variables")]
        [SerializeField] string description;
        public string Description => description;

        [Group("Variables")]
        [SerializeField] int requiredLevel;
        public int RequiredLevel => requiredLevel;

        [LineSpacer("UI")]
        [Group("UI")]
        [SerializeField] bool visualiseActiveState = false;
        public bool VisualiseActiveState => visualiseActiveState;

        [Group("UI")]
        [SerializeField] Color backgroundColor = Color.white;
        public Color BackgroundColor => backgroundColor;

        [LineSpacer("Purchase")]
        [Group("Purchase")]
        [SerializeField] PurchaseType purchaseOption;
        public PurchaseType PurchaseOption => purchaseOption;

        [Group("Purchase"), ShowIf("IsCurrencyPurchaseType")]
        [SerializeField] CurrencyType currencyType;
        public CurrencyType CurrencyType => currencyType;

        [Group("Purchase"), ShowIf("IsCurrencyPurchaseType")]
        [SerializeField] int price;
        public int Price => price;

        [Group("Purchase")]
        [SerializeField] int purchaseAmount;
        public int PurchaseAmount => purchaseAmount;

        [LineSpacer("Floating Text")]
        [SerializeField] string floatingMessage;
        public string FloatingMessage => floatingMessage;

        [System.NonSerialized]
        private CrystalPUSave save;
        public CrystalPUSave Save => save;

        public bool IsUnlocked { get => save.IsUnlocked; set => save.IsUnlocked = value; }

        public void InitialiseSave()
        {
            save = DataManager.GetSaveObject<CrystalPUSave>(string.Format("powerUp_{0}", type));

            if (save.Amount == -1)
                save.Amount = defaultAmount;
        }

        public abstract void Init();

        public bool HasEnoughCurrency()
        {
            return EconomyManager.HasAmount(currencyType, price);
        }

        private bool IsCurrencyPurchaseType()
        {
            return purchaseOption == PurchaseType.Currency || purchaseOption == PurchaseType.Both;
        }

        private bool IsVideoPurchaseType()
        {
            return purchaseOption == PurchaseType.RewardedVideo || purchaseOption == PurchaseType.Both;
        }


        public enum PurchaseType { Currency, RewardedVideo, Both }

    }
}
