using UnityEngine;

namespace CrystalUnbolt
{
    /// <summary>
    /// Your Custom Currency Manager
    /// Simple coins add/subtract system
    /// </summary>
    public class MyCurrencyManager : MonoBehaviour
    {
        public static MyCurrencyManager Instance { get; private set; }
        
        [SerializeField] private MyCurrencyConfig config;
        
        private int currentCoins;
        
        // Event when coins change
        public System.Action<int> OnCoinsChanged;
        
        public int CurrentCoins 
        { 
            get => currentCoins;
            private set
            {
                currentCoins = value;
                SaveCoins();
                OnCoinsChanged?.Invoke(currentCoins);
            }
        }
        
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            LoadCoins();
        }
        
        private void LoadCoins()
        {
            currentCoins = PlayerPrefs.GetInt("MyCoins", config != null ? config.startingCoins : 100);
            Debug.Log($"[MyCurrency] Loaded coins: {currentCoins}");
        }
        
        private void SaveCoins()
        {
            PlayerPrefs.SetInt("MyCoins", currentCoins);
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// Add coins to player
        /// </summary>
        public void AddCoins(int amount)
        {
            if (amount <= 0) return;
            
            CurrentCoins += amount;
            Debug.Log($"[MyCurrency] Added {amount} coins. Total: {CurrentCoins}");
        }
        
        /// <summary>
        /// Remove coins from player
        /// </summary>
        public bool RemoveCoins(int amount)
        {
            if (amount <= 0) return false;
            
            if (currentCoins >= amount)
            {
                CurrentCoins -= amount;
                Debug.Log($"[MyCurrency] Removed {amount} coins. Total: {CurrentCoins}");
                return true;
            }
            
            Debug.Log($"[MyCurrency] Not enough coins! Have: {currentCoins}, Need: {amount}");
            return false;
        }
        
        /// <summary>
        /// Check if player has enough coins
        /// </summary>
        public bool HasEnough(int amount)
        {
            return currentCoins >= amount;
        }
        
        /// <summary>
        /// Set coins to specific amount (for testing/admin)
        /// </summary>
        public void SetCoins(int amount)
        {
            CurrentCoins = Mathf.Max(0, amount);
            Debug.Log($"[MyCurrency] Set coins to: {CurrentCoins}");
        }
        
        /// <summary>
        /// Reset coins to starting value
        /// </summary>
        public void ResetCoins()
        {
            CurrentCoins = config != null ? config.startingCoins : 100;
            Debug.Log($"[MyCurrency] Reset coins to: {CurrentCoins}");
        }
    }
}






