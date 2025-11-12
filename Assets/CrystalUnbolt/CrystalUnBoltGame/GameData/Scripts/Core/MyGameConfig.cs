using UnityEngine;

namespace CrystalUnbolt
{
    /// <summary>
    /// Your Custom Game Configuration
    /// Core game settings
    /// </summary>
    [CreateAssetMenu(fileName = "My Game Config", menuName = "MyGame/Game Configuration")]
    public class MyGameConfig : ScriptableObject
    {
        [Header("=== Tutorial ===")]
        public bool showTutorial = true;
        
        [Header("=== Level Settings ===")]
        [Tooltip("Total levels available")]
        public int totalLevels = 117;
        
        [Tooltip("Levels can repeat infinitely?")]
        public bool infiniteLevels = true;
        
        [Header("=== Timer Settings ===")]
        public bool useGameTimer = true;
        [Tooltip("Seconds per level")]
        public float levelTime = 120f;
        
        [Header("=== Difficulty ===")]
        [Tooltip("Show hint after X seconds")]
        public float hintDelaySeconds = 30f;
        
        [Tooltip("Allow replaying failed level?")]
        public bool allowReplay = true;
        public int replayCost = 100; // in coins
        
        [Header("=== Rewards ===")]
        public int coinsPerLevel = 50;
        public int bonusForSpeed = 20; // If complete fast
        public int bonusForPerfect = 30; // No mistakes
        
        [Header("=== Ads Frequency ===")]
        [Tooltip("Show interstitial ad every X levels")]
        public int adEveryNLevels = 3;
        
        [Header("=== Debug ===")]
        public bool enableDevPanel = false;
        public bool skipToLevel = false;
        public int skipToLevelNumber = 1;
    }
}






