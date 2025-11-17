using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrystalUnbolt
{
    [System.Serializable]
    public class CrystalStageData
    {
        [SerializeField, LevelEditorSetting] List<CrystalHoleData> holePositions;
        public List<CrystalHoleData> HolePositions => holePositions;

        [SerializeField, LevelEditorSetting] List<CrystalPlankLevelData> planksData;
        public List<CrystalPlankLevelData> PlanksData => planksData;

        [SerializeField] List<CrystalLockedHoleConfig> lockedHoles;
        public List<CrystalLockedHoleConfig> LockedHoles => lockedHoles;

        [SerializeField] FloatToggle timerOverride;
        public bool TimerOverrideEnabled => timerOverride.Enabled;
        public float TimerOverride => timerOverride.Handle(0);
    }
}
