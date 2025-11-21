using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalUnbolt
{
    public class CrystalPUUIPurchasePanel : MonoBehaviour, IPopupWindow
    {
        [SerializeField] GameObject powerUpPurchasePanel;
        [SerializeField] RectTransform safeAreaTransform;

        [Space(5)]
        [SerializeField] Image powerUpPurchasePreview;
        [SerializeField] TMP_Text powerUpPurchaseAmountText;
        [SerializeField] TMP_Text powerUpPurchaseDescriptionText;
        [SerializeField] TMP_Text powerUpPurchasePriceText;
        [SerializeField] Image powerUpPurchaseIcon;

        [Space(5)]
        [SerializeField] Button smallCloseButton;
        [SerializeField] Button bigCloseButton;
        [SerializeField] Button purchaseButton;     
        [SerializeField] Button purchaseRVButton;   

        [Space(5)]
        [SerializeField] CrystalCurrencyUIPanelSimple currencyPanel;

        private CrystalPUSettings settings;

        public bool IsOpened => powerUpPurchasePanel.activeSelf;

        private void Awake()
        {
            smallCloseButton.onClick.AddListener(ClosePurchasePUPanel);
            bigCloseButton.onClick.AddListener(ClosePurchasePUPanel);
            purchaseButton.onClick.AddListener(PurchasePUButton);
            purchaseRVButton.onClick.AddListener(PurchaseRVButton);
        }

        public void Init()
        {
            SafeAreaHandler.RegisterRectTransform(safeAreaTransform);
        }

        public void Show(CrystalPUSettings settings)
        {
            this.settings = settings;

            currencyPanel.Init();

            powerUpPurchasePanel.SetActive(true);

            powerUpPurchasePreview.sprite = settings.Icon;
            powerUpPurchaseDescriptionText.text = settings.Description;
            powerUpPurchaseAmountText.text = $"x{settings.PurchaseAmount}";

            bool showCurrency = settings.PurchaseOption == CrystalPUSettings.PurchaseType.Currency
                                || settings.PurchaseOption == CrystalPUSettings.PurchaseType.Both;

            bool showRV = settings.PurchaseOption == CrystalPUSettings.PurchaseType.RewardedVideo
                          || settings.PurchaseOption == CrystalPUSettings.PurchaseType.Both;

            if (showCurrency)
            {
                powerUpPurchasePriceText.text = settings.Price.ToString();
                Currency currency = EconomyManager.GetCurrency(settings.CurrencyType);
                powerUpPurchaseIcon.sprite = currency.Icon;
            }

            powerUpPurchasePriceText.gameObject.SetActive(showCurrency);
            powerUpPurchaseIcon.gameObject.SetActive(showCurrency);

            purchaseButton.gameObject.SetActive(showCurrency);
            purchaseRVButton.gameObject.SetActive(showRV);

            ScreenManager.OnPopupWindowOpened(this);

            // Show power icon with final scale of 2 after animation
            PlayPowerIconAnimation();

            // Show buttons with default scale (1)
            if (showCurrency) PopupHelper.ShowPopup(purchaseButton.transform);
            if (showRV) PopupHelper.ShowPopup(purchaseRVButton.transform);
        }

        public void PurchasePUButton()
        {
            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
           Haptic.Play(Haptic.HAPTIC_HARD);
#endif
            bool purchaseSuccessful = CrystalPUController.PurchasePowerUp(settings.Type);

            if (purchaseSuccessful)
                ClosePurchasePUPanel();
        }

        public void PurchaseRVButton()
        {
            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
           Haptic.Play(Haptic.HAPTIC_HARD);
#endif
#if MODULE_MONETIZATION
            MyAdsAdapter.ShowRewardBasedVideo((bool reward) =>
            {
                if (reward)
                {
                    CrystalPUController.AddPowerUp(settings.Type, settings.PurchaseAmount);
                    ClosePurchasePUPanel();
                }
            });
#else
            Debug.LogWarning("Monetization module is missing!");

            CrystalPUController.AddPowerUp(settings.Type, settings.PurchaseAmount);
            ClosePurchasePUPanel();
#endif
        }
        private void PlayPowerIconAnimation()
        {
            Transform icon = powerUpPurchasePreview.transform;

            // kill any previous tweens on icon
            icon.DOKill(true);

            icon.localScale = Vector3.zero;
            icon.localRotation = Quaternion.identity;

            var seq = DG.Tweening.DOTween.Sequence();

            // POP + bounce
            seq.Append(
                DG.Tweening.ShortcutExtensions.DOScale(icon, 2.25f, 0.25f)
                    .SetEase(DG.Tweening.Ease.OutBack)
            );

            // settle to normal size
            seq.Append(
                DG.Tweening.ShortcutExtensions.DOScale(icon, 2f, 0.18f)
                    .SetEase(DG.Tweening.Ease.OutSine)
            );
        }



        public void ClosePurchasePUPanel()
        {
            powerUpPurchasePanel.SetActive(false);
#if MODULE_HAPTIC
           Haptic.Play(Haptic.HAPTIC_HARD);
#endif
            ScreenManager.OnPopupWindowClosed(this);
        }
    }
}
