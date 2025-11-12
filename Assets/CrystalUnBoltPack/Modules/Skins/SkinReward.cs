using UnityEngine;

namespace CrystalUnbolt
{
    public class SkinReward : CrystalReward
    {
        [SkinPicker]
        [SerializeField] string skinID;

        [SerializeField] bool disableIfSkinIsUnlocked;

        private CrystalSkinController skinsController;

        private void Start()
        {
            skinsController = CrystalSkinController.Instance;
        }

        private void OnEnable()
        {
            CrystalSkinController.SkinUnlocked += OnSkinUnlocked;    
        }

        private void OnDisable()
        {
            CrystalSkinController.SkinUnlocked -= OnSkinUnlocked;
        }

        public override void ApplyReward()
        {
            skinsController.UnlockSkin(skinID, true);
        }

        public override bool CheckDisableState()
        {
            if(disableIfSkinIsUnlocked)
            {
                return skinsController.IsSkinUnlocked(skinID);
            }

            return false;
        }

        private void OnSkinUnlocked(ISkinData skinData)
        {
            if(disableIfSkinIsUnlocked)
            {
                if(skinData.ID == skinID)
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }
}
