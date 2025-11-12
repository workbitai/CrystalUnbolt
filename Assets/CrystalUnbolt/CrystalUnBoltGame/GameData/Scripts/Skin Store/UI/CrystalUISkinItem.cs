using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalUnbolt.SkinStore
{
    public class CrystalUISkinItem : MonoBehaviour
    {
        [SerializeField] Button button;
        [SerializeField] Image productImage;
        [SerializeField] Image productMaskImage;

        [Space]
        [SerializeField] Image costOutline;
        [SerializeField] Image costBackground;
        [SerializeField] Image currencyImage;
        [SerializeField] TextMeshProUGUI costText;
        

        [Space]
        [SerializeField] Color inGamePurchaseTypeBackColor;
        [SerializeField] Color rewardedPurchaseTypeBackColor;

        [SerializeField] Color availableCostColor;
        [SerializeField] Color notAvailableCostColor;

        [Space]
        [SerializeField] Image selectionOutlineImage;

        [Space]
        [SerializeField] SimpleBounce bounce;

        public CrystalSkinStoreController Controller { get; private set; }
        public CrystalSkinStoreProductContainer Data { get; private set; }

        public bool IsSelected { get; private set; }

        public void Init(CrystalSkinStoreController controller, CrystalSkinStoreProductContainer data, bool selected)
        {
            Data = data;

            Controller = controller;

            bounce.Init(transform);

            if (data.ProductData.IsDummy)
            {
                productImage.sprite = data.ProductData.LockedSprite;
                costOutline.gameObject.SetActive(false);
            }
            else
            {
                if (data.IsUnlocked)
                {
                    productImage.sprite = data.ProductData.OpenedSprite;
                    costOutline.gameObject.SetActive(false);
                    button.enabled = true;
                }
                else
                {
                    productImage.sprite = data.ProductData.LockedSprite;
                    costOutline.gameObject.SetActive(true);


                    if (data.ProductData.PurchType == CrystalSkinStoreProductData.PurchaseType.InGameCurrency)
                    {
                        costBackground.color = inGamePurchaseTypeBackColor;
                        currencyImage.sprite = EconomyManager.GetCurrency(data.ProductData.Currency).Icon;
                        costText.text = data.ProductData.Cost.ToString();
                    }
                    else
                    {
                        costBackground.color = rewardedPurchaseTypeBackColor;
                        currencyImage.sprite = ScreenManager.GetPage<CrystalUISkinStore>().AdsIcon;
                        costText.text = data.ProductData.RewardedVideoWatchedAmount + "/" + data.ProductData.Cost.ToString();

                        button.enabled = true;
                    }

                    UpdatePriceText();
                }
            }

            productMaskImage.color = Controller.SelectedTabData.ProductBackgroundColor;

            SetSelectedStatus(selected);
        }

        public void SetSelectedStatus(bool isSelected)
        {
            IsSelected = isSelected;

            if (Data.ProductData.IsDummy || isSelected)
            {
                button.enabled = false;
            }
            else if (Data.IsUnlocked)
            {
                button.enabled = true;
            }
            else if (Data.ProductData.PurchType == CrystalSkinStoreProductData.PurchaseType.RewardedVideo)
            {
                button.enabled = true;
            }
            else
            {
                button.enabled = EconomyManager.HasAmount(Data.ProductData.Currency, Data.ProductData.Cost);
            }

            selectionOutlineImage.gameObject.SetActive(isSelected);
        }

        public void UpdatePriceText()
        {
            if (Data.ProductData.PurchType == CrystalSkinStoreProductData.PurchaseType.InGameCurrency)
            {
                costText.color = EconomyManager.HasAmount(Data.ProductData.Currency, Data.ProductData.Cost) ? availableCostColor : notAvailableCostColor;
            }
            else
            {
                costText.color = availableCostColor;
                costText.text = Data.ProductData.RewardedVideoWatchedAmount + "/" + Data.ProductData.Cost;
            }    
        }

        public void OnButtonClicked()
        {
            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif
            bounce.Bounce();

            if (Data.IsUnlocked)
            {
                Controller.SelectProduct(Data);
            }
            else
            {
                Controller.BuyProduct(Data);
            }
        }
    }
}