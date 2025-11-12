using UnityEngine;
using UnityEngine.UI;

namespace CrystalUnbolt
{
    public class CrystalUINoAdsPopUp : MonoBehaviour, IPopupWindow
    {
        [SerializeField] UIScaleAnimation panelScalable;
        [SerializeField] UIFadeAnimation backFade;
        [SerializeField] Button bigCloseButton;
        [SerializeField] Button smallCloseButton;
        // [SerializeField] CrystalIAPButton removeAdsButton; // IAP Removed!
        [SerializeField] RectTransform noadsIcon;

        public bool IsOpened => gameObject.activeSelf;

        private void Awake()
        {
            bigCloseButton.onClick.AddListener(ClosePanel);
            smallCloseButton.onClick.AddListener(ClosePanel);

            // IAPManager.SubscribeOnPurchaseModuleInitted(OnPurchaseModuleInitted); // IAP Disabled!

            backFade.Hide(immediately: true);
            panelScalable.Hide(immediately: true);
        }
        // IAP Methods - DISABLED (IAP System Removed!)
        /*
        private void OnPurchaseModuleInitted()
        {
            Debug.Log("[UINoAdsPopUp] OnPurchaseModuleInitted called");
            
            if (removeAdsButton != null)
            {
                try
                {
                    Debug.Log("[UINoAdsPopUp] Initializing removeAdsButton...");
                    removeAdsButton.Init(ProductKeyType.NoAds);
                    Debug.Log("[UINoAdsPopUp] removeAdsButton initialized successfully");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[UINoAdsPopUp] Failed to initialize removeAdsButton: {e.Message}");
                    Debug.LogError($"[UINoAdsPopUp] Stack trace: {e.StackTrace}");
                    
                    if (removeAdsButton.gameObject != null)
                    {
                        removeAdsButton.gameObject.SetActive(true);
                        var buttonComponent = removeAdsButton.GetComponent<UnityEngine.UI.Button>();
                        if (buttonComponent != null)
                        {
                            buttonComponent.interactable = false;
                        }
                        Debug.Log("[UINoAdsPopUp] Button shown but disabled due to IAP error");
                    }
                }
            }
            else
            {
                Debug.LogError("[UINoAdsPopUp] removeAdsButton is null!");
            }
        }

        private void PurcaseCompleted(ProductKeyType productKeyType)
        {
            if(productKeyType == ProductKeyType.NoAds)
            {
                MyAdsAdapter.DisableForcedAd();

                ClosePanel();
            }
        }
        */

        public void Show()
        {
            bigCloseButton.interactable = true;
            smallCloseButton.interactable = true;

            gameObject.SetActive(true);
            backFade.Show(0.2f, onCompleted: () =>
            {
                panelScalable.Show(immediately: false, duration: 0.3f);
            });
            PopupHelper.ShowPopup(noadsIcon);
            // PopupHelper.ShowPopup(removeAdsButton.transform); // IAP Button removed!

            // IAPManager.PurchaseCompleted += PurcaseCompleted; // IAP Disabled!

            ScreenManager.OnPopupWindowOpened(this);
        }

        private void ClosePanel()
        {
            bigCloseButton.interactable = false;
            smallCloseButton.interactable = false;
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif
            backFade.Hide(0.2f);
            panelScalable.Hide(immediately: false, duration: 0.4f, onCompleted: () =>
            {
                gameObject.SetActive(false);
            });

            // IAPManager.PurchaseCompleted -= PurcaseCompleted; // IAP Disabled!

            ScreenManager.OnPopupWindowClosed(this);
        }
    }
}