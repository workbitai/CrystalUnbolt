using UnityEngine;

namespace CrystalUnbolt
{
    [CreateAssetMenu(fileName = "Game Data", menuName = "Content/Game Data")]
    public class CrystalGameData : ScriptableObject
    {
        [SerializeField] bool showTutorial = true;
        public bool ShowTutorial => showTutorial;

        [SerializeField] bool infiniteLevels;
        public bool InfiniteLevels => infiniteLevels;

        [Header("Gameplay")]

        [SerializeField] FloatToggle CrystalGameTimer;
        public bool GameplayTimerEnabled => CrystalGameTimer.Enabled;
        public float GameplayTimerValue => CrystalGameTimer.Value;

        [SerializeField] int coinsRewardPerLevel = 50;
        public int CoinsRewardPerLevel => coinsRewardPerLevel;

        [Tooltip("0 - zero reward; 100 - full reward")]
        [SerializeField] IntToggle replayingLevelRewardPercent = new IntToggle(true, 20);
        public IntToggle ReplayingLevelRewardPercent => replayingLevelRewardPercent;

        [SerializeField] int replayStageCost;
        public int ReplayStageCost => replayStageCost;

        [Header("Screw")]
        [SerializeField] Color screwShadowColor = Color.black;
        public Color ScrewShadowColor => screwShadowColor;

        [SerializeField] Color screwHighlightColor = Color.green;
        public Color ScrewHighlightColor => screwHighlightColor;

        [SerializeField] float highlightSizeMultiplier = 1.4f;
        public float HighlightSizeMultiplier => highlightSizeMultiplier;

        [SerializeField] bool highlightBreathingEffect;
        public bool HighlightBreathingEffect => highlightBreathingEffect;

        [SerializeField] float screwMovementDuration = 0.2f;
        public float ScrewMovementDuration => screwMovementDuration;

        [SerializeField] float crystalExtractionAnimationSpeedMultiplier = 1;
        public float CrystalExtractionAnimationSpeedMultiplier => crystalExtractionAnimationSpeedMultiplier;

        [SerializeField] float screwInAnimationSpeedMultiplier = 1;
        public float ScrewInAnimationSpeedMultiplier => screwInAnimationSpeedMultiplier;

        [SerializeField] float crystalExtractionPUAnimationSpeedMultiplier = 1;
        public float CrystalExtractionPUAnimationSpeedMultiplier => crystalExtractionPUAnimationSpeedMultiplier;

        [SerializeField, Range(0.5f, 1f)] float holeVisibleAmountToEnableScrew = 0.9f;
        public float HoleVisibleAmountToEnableScrew => holeVisibleAmountToEnableScrew;

        [Header("Ads")]

        [SerializeField] float additionalTimerTimeOnFail = 30;
        public float AdditionalTimerTimeOnFail => additionalTimerTimeOnFail;

        private void OnValidate()
        {

        }
    }
}
