using System.Collections.Generic;
using UnityEngine;

namespace CrystalUnbolt
{
   
    public static class CrystalCloudProgressSync
    {
        private static CrystalLoginAuthManager _auth;
        private static CrystalPlayerDataService _data;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (_auth == null) _auth = Object.FindObjectOfType<CrystalLoginAuthManager>();
            if (_data == null) _data = Object.FindObjectOfType<CrystalPlayerDataService>();
        }

        public static void Inject(CrystalLoginAuthManager auth, CrystalPlayerDataService data)
        {
            _auth = auth;
            _data = data;
        }

     
        public static void ApplyCloudToLocal(PlayerData p)
        {
            if (p == null) return;

            var CrystalLevelSave = DataManager.GetSaveObject<CrystalLevelSave>("level");
            int cloudMaxIndex = Mathf.Max(0, p.level - 1);
            CrystalLevelSave.MaxReachedLevelIndex = cloudMaxIndex;
            CrystalLevelSave.DisplayLevelIndex = -1;
            CrystalLevelSave.RealLevelIndex = -1;
            CrystalLevelSave.IsPlayingRandomLevel = false;
            DataManager.Save(false);

            try
            {
                int localCoins = EconomyManager.Get(CurrencyType.Coins);
                int delta = p.coins - localCoins;
                if (delta != 0)
                {
                    if (delta > 0) EconomyManager.Add(CurrencyType.Coins, delta);
                    else EconomyManager.Substract(CurrencyType.Coins, -delta);
                    DataManager.Save(false);
                }
            }
            catch {  }
        }

      
        public static async void PushLocalToCloud()
        {
            if (_auth == null) _auth = Object.FindObjectOfType<CrystalLoginAuthManager>();
            if (_data == null) _data = Object.FindObjectOfType<CrystalPlayerDataService>();
            if (_auth == null || _data == null || _auth.CurrentUser == null) return;

            var CrystalLevelSave = DataManager.GetSaveObject<CrystalLevelSave>("level");
            int levelOneBased = Mathf.Max(1, CrystalLevelSave.MaxReachedLevelIndex + 1);
            int coins = 0;
            try { coins = EconomyManager.Get(CurrencyType.Coins); } catch { }

            var patch = new Dictionary<string, object>
            {
                ["level"] = levelOneBased,
                ["coins"] = coins,
                ["updatedAt"] = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            await _data.UpdateFields(_auth.CurrentUser.UserId, patch);
        }
    }
}


