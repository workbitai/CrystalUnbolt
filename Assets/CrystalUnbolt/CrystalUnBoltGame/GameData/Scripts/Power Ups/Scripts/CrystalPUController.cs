using System.Collections.Generic;
using UnityEngine;
using CrystalUnbolt.IAPStore;

namespace CrystalUnbolt
{
    [StaticUnload]
    public class CrystalPUController : MonoBehaviour
    {
        private static CrystalPUController instance;

        [DrawReference]
        [SerializeField] CrystalPUDatabase database;

        [LineSpacer("Sounds")]
        [SerializeField] AudioClip activateSound;

        public static CrystalPUBehavior[] ActivePowerUps { get; private set; }
        public static CrystalPUUIController PowerUpsUIController { get; private set; }
        public static CrystalPUBehavior SelectedPU { get; private set; }

        private static Dictionary<CrystalPUType, CrystalPUBehavior> powerUpsLink;

        public static event PowerUpCallback Used;
        public static event PowerUpCallback Unlocked;

        private Transform behaviorsContainer;

        public void Init()
        {
#if MODULE_POWERUPS
            instance = this;

            behaviorsContainer = new GameObject("[POWER UPS]").transform;
            behaviorsContainer.gameObject.isStatic = true;

            CrystalPUSettings[] powerUpSettings = database.PowerUps;
            ActivePowerUps = new CrystalPUBehavior[powerUpSettings.Length];
            powerUpsLink = new Dictionary<CrystalPUType, CrystalPUBehavior>();

            for (int i = 0; i < ActivePowerUps.Length; i++)
            {
                powerUpSettings[i].InitialiseSave();
                powerUpSettings[i].Init();

                GameObject powerUpBehaviorObject = Instantiate(powerUpSettings[i].BehaviorPrefab, behaviorsContainer);
                powerUpBehaviorObject.transform.ResetLocal();

                CrystalPUBehavior powerUpBehavior = powerUpBehaviorObject.GetComponent<CrystalPUBehavior>();
                powerUpBehavior.InitialiseSettings(powerUpSettings[i]);
                powerUpBehavior.Init();

                ActivePowerUps[i] = powerUpBehavior;

                powerUpsLink.Add(ActivePowerUps[i].Settings.Type, ActivePowerUps[i]);
            }

            CrystalUIGame gameUI = ScreenManager.GetPage<CrystalUIGame>();

            PowerUpsUIController = gameUI.PowerUpsUIController;
            PowerUpsUIController.Initialise(this);
#else
            Debug.LogError("[PU Controller]: Module Define isn't active!");
#endif
        }

        public static bool PurchasePowerUp(CrystalPUType powerUpType)
        {
            if (powerUpsLink.ContainsKey(powerUpType))
            {
                CrystalPUBehavior powerUpBehavior = powerUpsLink[powerUpType];
                if(powerUpBehavior.Settings.HasEnoughCurrency())
                {
                    EconomyManager.Substract(powerUpBehavior.Settings.CurrencyType, powerUpBehavior.Settings.Price);

                    powerUpBehavior.Settings.Save.Amount += powerUpBehavior.Settings.PurchaseAmount;

                    PowerUpsUIController.RedrawPanels();

                    return true;
                }
                else
                {
                    ScreenManager.DisplayScreen<CrystalUIStore>();
                    MyAdsAdapter.DisableBanner();
                    return false;
                }
            }
            else
            {
                Debug.LogWarning(string.Format("[Power Ups]: Power up with type {0} isn't registered.", powerUpType));
            }

            return false;
        }

        public static void AddPowerUp(CrystalPUType powerUpType, int amount)
        {
            if (powerUpsLink.ContainsKey(powerUpType))
            {
                CrystalPUBehavior powerUpBehavior = powerUpsLink[powerUpType];

                powerUpBehavior.Settings.Save.Amount += amount;

                PowerUpsUIController.RedrawPanels();
            }
            else
            {
                Debug.LogWarning(string.Format("[Power Ups]: Power up with type {0} isn't registered.", powerUpType));
            }
        }

        public static void SetPowerUpAmount(CrystalPUType powerUpType, int amount)
        {
            if (powerUpsLink.ContainsKey(powerUpType))
            {
                CrystalPUBehavior powerUpBehavior = powerUpsLink[powerUpType];

                powerUpBehavior.Settings.Save.Amount = amount;

                PowerUpsUIController.RedrawPanels();
            }
            else
            {
                Debug.LogWarning(string.Format("[Power Ups]: Power up with type {0} isn't registered.", powerUpType));
            }
        }

        public static bool SelectPowerUp(CrystalPUType powerUpType)
        {
            if (powerUpsLink.ContainsKey(powerUpType))
            {
                CrystalPUBehavior powerUpBehavior = powerUpsLink[powerUpType];

                if(SelectedPU == powerUpBehavior)
                {
                    SelectedPU.OnDeselected(); 
                    SelectedPU = null;

                    return false;
                }

                if (!powerUpBehavior.IsBusy)
                {
                    if(SelectedPU != null)
                    {
                        SelectedPU.OnDeselected();
                        SelectedPU = null;
                    }

                    powerUpBehavior.OnSelected();

                    SelectedPU = powerUpBehavior;

                    return true;
                }
            }
            else
            {
                Debug.LogWarning(string.Format("[Power Ups]: Power up with type {0} isn't registered.", powerUpType));
            }

            return false;
        }

        public static void ApplyToElement(IClickableObject clickableObject, Vector3 clickPosition)
        {
            if(SelectedPU != null)
            {
                if(SelectedPU.ApplyToElement(clickableObject, clickPosition))
                {
                    CrystalPUSettings settings = SelectedPU.Settings;

                    SoundManager.PlaySound(settings.CustomAudioClip.Handle(instance.activateSound));

                    settings.Save.Amount--;

                    PowerUpsUIController.OnPowerUpUsed(SelectedPU);

                    Used?.Invoke(settings.Type);
                }

                SelectedPU.OnDeselected();
                SelectedPU = null;
            }
        }

        public static void UnselectPowerUp()
        {
            if(SelectedPU != null)
            {
                SelectedPU.OnDeselected();
                SelectedPU = null;
            }
        }

        public static bool UsePowerUp(CrystalPUType powerUpType)
        {
            if(powerUpsLink.ContainsKey(powerUpType))
            {
                CrystalPUBehavior powerUpBehavior = powerUpsLink[powerUpType];
                if(!powerUpBehavior.IsBusy)
                {
                    if(powerUpBehavior.Activate())
                    {
                        CrystalPUSettings settings = powerUpBehavior.Settings;

                        SoundManager.PlaySound(settings.CustomAudioClip.Handle(instance.activateSound));
#if MODULE_HAPTIC
                        Haptic.Play(Haptic.HAPTIC_HARD);
#endif
                        settings.Save.Amount--;

                        PowerUpsUIController.OnPowerUpUsed(powerUpBehavior);

                        Used?.Invoke(powerUpType);

                        return true;
                    }
                }
            }
            else
            {
                Debug.LogWarning(string.Format("[Power Ups]: Power up with type {0} isn't registered.", powerUpType));
            }

            return false;
        }

        public static void ResetPowerUp(CrystalPUType powerUpType)
        {
            if (powerUpsLink.ContainsKey(powerUpType))
            {
                CrystalPUBehavior powerUpBehavior = powerUpsLink[powerUpType];

                powerUpBehavior.Settings.Save.Amount = 0;

                PowerUpsUIController.RedrawPanels();
            }
            else
            {
                Debug.LogWarning(string.Format("[Power Ups]: Power up with type {0} isn't registered.", powerUpType));
            }
        }

        public static void UnlockPowerUp(CrystalPUType powerUpType)
        {
            if (powerUpsLink.ContainsKey(powerUpType))
            {
                CrystalPUBehavior powerUpBehavior = powerUpsLink[powerUpType];
                CrystalPUSettings settings = powerUpBehavior.Settings;

                if(!settings.IsUnlocked)
                {
                    settings.IsUnlocked = true;

                    Unlocked?.Invoke(powerUpType);
                }
            }
        }

        public static void ResetPowerUps()
        {
            foreach(CrystalPUBehavior powerUp in ActivePowerUps)
            {
                powerUp.Settings.Save.Amount = 0;
            }

            PowerUpsUIController.RedrawPanels();
        }

        public static CrystalPUBehavior GetPowerUpBehavior(CrystalPUType powerUpType)
        {
            if (powerUpsLink.ContainsKey(powerUpType))
            {
                return powerUpsLink[powerUpType];
            }

            Debug.LogWarning(string.Format("[Power Ups]: Power up with type {0} isn't registered.", powerUpType));

            return null;
        }

        public static void ResetBehaviors()
        {
            for(int i = 0; i < ActivePowerUps.Length; i++)
            {
                ActivePowerUps[i].ResetBehavior();
            }
        }

        [Button("Give Test Amount")]
        public void GiveDebugAmount()
        {
            if (!Application.isPlaying) return;

            for(int i = 0; i < ActivePowerUps.Length; i++)
            {
                ActivePowerUps[i].Settings.Save.Amount = 999;
            }

            PowerUpsUIController.RedrawPanels();
        }

        [Button("Reset Amount")]
        public void ResetDebugAmount()
        {
            if (!Application.isPlaying) return;

            for (int i = 0; i < ActivePowerUps.Length; i++)
            {
                ActivePowerUps[i].Settings.Save.Amount = 0;
            }

            PowerUpsUIController.RedrawPanels();
        }

        private static void UnloadStatic()
        {
            Used = null;
            Unlocked = null;

            powerUpsLink = null;

            ActivePowerUps = null;
            PowerUpsUIController = null;
            SelectedPU = null;
        }

        public delegate void PowerUpCallback(CrystalPUType powerUpType);
    }
}

