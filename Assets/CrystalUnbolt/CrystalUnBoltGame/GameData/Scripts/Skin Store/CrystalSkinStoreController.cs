using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrystalUnbolt.SkinStore
{
    public class CrystalSkinStoreController : MonoBehaviour
    {
        private static CrystalSkinStoreController instance;
        public static CrystalSkinStoreController Instance => instance;

        [SerializeField] CrystalSkinStoreDatabase database;
        public CrystalSkinStoreDatabase Database => instance.database;

        private Dictionary<TabData, List<CrystalSkinStoreProductContainer>> products;

        public int TabsCount => Database.Tabs.Length;
        public int CoinsForAdsAmount => Database.CoinsForAds;
        public CurrencyType CoinsForAdsCurrency => Database.CurrencyForAds;

        public TabData SelectedTabData { get; private set; }

        private CrystalUISkinStore storeUI;

        public ISkinsProvider SkinsProvider { get; private set; }

        public void Init(ISkinsProvider skinsProvider)
        {
            instance = this;

            SkinsProvider = skinsProvider;

            if (database == null)
            {
                Debug.LogError("[CrystalSkinStoreController] Database is not assigned in the inspector!");
                return;
            }

            Dictionary<TabData, List<CrystalSkinStoreProductData>> rawProducts = Database.Init();
            products = new Dictionary<TabData, List<CrystalSkinStoreProductContainer>>();

            foreach (TabData tab in rawProducts.Keys)
            {
                List<CrystalSkinStoreProductData> productsList = rawProducts[tab];

                List<CrystalSkinStoreProductContainer> containersInsideTab = new List<CrystalSkinStoreProductContainer>();
                for(int i = 0; i < productsList.Count; i++)
                {
                    CrystalSkinStoreProductData product = productsList[i];

                    ISkinData skinData = null;
                    if (!product.IsDummy)
                    {
                        skinData = SkinsProvider.GetSkinData(product.SkinId);
                    }

                    var container = new CrystalSkinStoreProductContainer(product, skinData);

                    containersInsideTab.Add(container);
                }

                products.Add(tab, containersInsideTab);
            }

            storeUI = ScreenManager.GetPage<CrystalUISkinStore>();
            if (storeUI != null)
            {
                storeUI.InitTabs(OnTabClicked);
            }
            else
            {
                Debug.LogWarning("[CrystalSkinStoreController] CrystalUISkinStore not found in ScreenManager. Skin store UI will not be initialized.");
            }

            InitDefaultProducts();

            if (Database.Tabs != null && Database.Tabs.Length > 0)
            {
                SelectedTabData = Database.Tabs[0];
            }
        }

        private void InitDefaultProducts()
        {
            var visitedTypes = new List<SkinTab>();

            foreach (var tab in products.Keys)
            {
                if (visitedTypes.Contains(tab.Type))
                    continue;
                visitedTypes.Add(tab.Type);

                var page = products[tab];

                if (page.Count > 0)
                {
                    var defaultContainer = page[0];
                    SkinsProvider.UnlockSkin(defaultContainer.SkinData);
                }
            }
        }

        public TabData GetTab(int tabId)
        {
            return Database.Tabs[tabId];
        }

        public List<CrystalSkinStoreProductContainer> GetProducts(TabData tab)
        {
            return products[tab];
        }

        public int PagesCount(TabData tab)
        {
            return products[tab].Count;
        }

        private void OnTabClicked(TabData data)
        {
            SelectedTabData = data;

            if (storeUI != null)
            {
                storeUI.SetSelectedTab(data);
            }
        }

        private void UnlockAndSelect(CrystalSkinStoreProductContainer container, bool select = true)
        {
            SkinsProvider.UnlockSkin(container.SkinData);
            if (select) SelectProduct(container);
        }

        public bool SelectProduct(CrystalSkinStoreProductContainer container)
        {
            if (!container.IsUnlocked)
                return false;

            SkinsProvider.SelectSkin(container.SkinData);

            if (storeUI != null)
            {
                storeUI.InitStoreUI();
            }

            return true;
        }

        public bool BuyProduct(CrystalSkinStoreProductContainer container, bool select = true, bool free = false)
        {
            if (container.IsUnlocked)
                return SelectProduct(container);

            if (free)
            {
                UnlockAndSelect(container, select);
                return true;
            }
            else if (container.ProductData.PurchType == CrystalSkinStoreProductData.PurchaseType.InGameCurrency && EconomyManager.HasAmount(container.ProductData.Currency, container.ProductData.Cost))
            {
                EconomyManager.Substract(container.ProductData.Currency, container.ProductData.Cost);
                UnlockAndSelect(container, select);
                return true;
            }
            // note | this type can't return true or false because result is not defined during execution of this code
            // right now result of this method is not used, but otherwise this logic needs to be improved
            else if(container.ProductData.PurchType == CrystalSkinStoreProductData.PurchaseType.RewardedVideo)
            {
                MyAdsAdapter.ShowRewardBasedVideo((success) =>
                {
                    if(success)
                    {
                        container.ProductData.RewardedVideoWatchedAmount++;

                        if(container.ProductData.RewardedVideoWatchedAmount >= container.ProductData.Cost)
                        {
                            UnlockAndSelect(container, select);
                        }

                        if (storeUI != null)
                        {
                            storeUI.InitStoreUI();
                        }
                    }
                });
            }

            return false;
        }

        public CrystalSkinStoreProductData GetRandomLockedProduct()
        {
            CrystalSkinStoreProductData lockedProduct = null;

            Database.Tabs.FindRandomOrder(tab =>
            {
                var product = products[tab].FindRandomOrder(product =>
                {
                    return !product.IsUnlocked && !product.ProductData.IsDummy;
                });

                if (product != null)
                {
                    lockedProduct = product.ProductData;
                    return true;
                }

                return false;
            });

            return lockedProduct;
        }

        public CrystalSkinStoreProductData GetRandomUnlockedProduct(SkinTab tab)
        {
            return products[GetTab((int)tab)].FindRandomOrder(product =>
            {
                return product.IsUnlocked && !product.ProductData.IsDummy;
            }).ProductData;
        }

        public CrystalSkinStoreProductData GetRandomProduct(SkinTab tab)
        {
            return products[GetTab((int)tab)].GetRandomItem().ProductData;
        }

        public CrystalSkinStoreProductContainer GetSelectedProductContainer()
        {
            List<CrystalSkinStoreProductContainer> selectedTabContainers = products[SelectedTabData];

            for(int i = 0; i < selectedTabContainers.Count; i++)
            {
                CrystalSkinStoreProductContainer container = selectedTabContainers[i];

                if(SkinsProvider.IsSkinSelected(container.SkinData)) return container;
            }

            return null;
        }
    }
}