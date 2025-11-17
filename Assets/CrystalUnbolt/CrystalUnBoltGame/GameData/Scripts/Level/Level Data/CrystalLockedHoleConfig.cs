using UnityEngine;

namespace CrystalUnbolt
{
    public enum CrystalHoleUnlockType
    {
        None = 0,
        RewardedAd = 1
    }

    [System.Serializable]
    public class CrystalLockedHoleConfig
    {
        [Tooltip("Enable/disable the lock entry without removing it from the list")]
        [SerializeField] private bool enabled = true;
        public bool Enabled => enabled;

        [Tooltip("Index of the hole in StageData.HolePositions that should start locked (0-based).")]
        [SerializeField] private int holeIndex = -1;
        public int HoleIndex => holeIndex;

        [Tooltip("Offset (local) for the unlock icon relative to the hole center.")]
        [SerializeField] private Vector2 iconOffset = new Vector2(0.35f, 0.35f);
        public Vector2 IconOffset => iconOffset;

        [Tooltip("Scale multiplier applied to the unlock icon.")]
        [SerializeField] private float iconScale = 1f;
        public float IconScale => iconScale;

        [Tooltip("Radius used for the unlock icon click area.")]
        [SerializeField] private float clickRadius = 0.32f;
        public float ClickRadius => clickRadius;

        [Tooltip("Which unlock flow to use.")]
        [SerializeField] private CrystalHoleUnlockType unlockType = CrystalHoleUnlockType.RewardedAd;
        public CrystalHoleUnlockType UnlockType => unlockType;

        [Tooltip("Optional placement identifier for analytics/debugging.")]
        [SerializeField] private string rewardPlacementId = "unlock_hole";
        public string RewardPlacementId => rewardPlacementId;
    }
}

