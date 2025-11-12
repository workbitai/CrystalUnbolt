using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrystalUnbolt.SkinStore
{
    public class CrystalSkinStoreProductContainer
    {
        public CrystalSkinStoreProductData ProductData { get; private set; }
        public ISkinData SkinData { get; private set; }

        public bool IsUnlocked => ProductData.IsDummy ? false : SkinData.IsUnlocked;

        public CrystalSkinStoreProductContainer(CrystalSkinStoreProductData data, ISkinData skinData)
        {
            ProductData = data;
            SkinData = skinData;
        }
    }
}
