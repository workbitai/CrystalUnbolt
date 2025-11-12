using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalUnbolt
{
    [System.Serializable]
    public class CrystalCurrencyUIPanelSimple : MonoBehaviour
    {
        [SerializeField] CurrencyType currencyType;

        [Space]
        [SerializeField] bool updateOnChange = true;
        [SerializeField] bool useFormattedAmount = true;

        [Space]
        [SerializeField] TextMeshProUGUI text;
        [SerializeField] Image icon;
        [SerializeField] Button addButton;

        public string Text { get => text.text; set => text.text = value; }
        public Sprite Icon { get => icon.sprite; set => icon.sprite = value; }

        public Image Image => icon;
        public Button AddButton => addButton;

        private Currency currency;
        public Currency Currency => currency;
        
        private RectTransform rectTransformRef;
        public RectTransform RectTransform => rectTransformRef;

        private bool isInitialized;

        private void Awake()
        {
            rectTransformRef = GetComponent<RectTransform>();

            Init();
        }

        public void Init()
        {
            if (isInitialized) return;

            currency = EconomyManager.GetCurrency(currencyType);
            
            if (currency == null)
            {
                Debug.LogError($"[CurrencyUIPanel] Currency of type {currencyType} not found! Make sure EconomyManager is properly initialized.");
                return;
            }

            if (icon != null)
                icon.sprite = currency.Icon;

            isInitialized = true;

            Redraw();
            Activate();
        }

        public void Init(CurrencyType currencyType)
        {
            if(isInitialized)
            {
                isInitialized = false;

                Disable();
            }

            this.currencyType = currencyType;

            Init();
        }

        public void Redraw()
        {
            if (currency == null || text == null)
            {
                Debug.LogWarning("[CurrencyUIPanel] Cannot redraw - currency or text is null");
                return;
            }
            
            text.text = useFormattedAmount ? currency.AmountFormatted : currency.Amount.ToString();
            Debug.Log("ADD Currency" + text.text);
        }

        public void SetAmount(int amount, bool format = true)
        {
            text.text = format ? CurrencyHelper.Format(amount) : amount.ToString();
            Debug.Log("ADD Currency" + text.text);
        }

        public void Activate()
        {
            if(updateOnChange && currency != null)
            {
                currency.OnCurrencyChanged += OnCurrencyAmountChanged;
            }
        }

        public void Disable()
        {
            if(updateOnChange && currency != null)
            {
                currency.OnCurrencyChanged -= OnCurrencyAmountChanged;
            }
        }

        private void OnCurrencyAmountChanged(Currency currency, int amountDifference)
        {
            text.text = useFormattedAmount ? currency.AmountFormatted : currency.Amount.ToString();
            //Debug.Log("ADD Currency" + text.text);
        }
    }
}