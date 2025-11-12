using System.Collections.Generic;
using UnityEngine;

namespace CrystalUnbolt
{
    [System.Serializable]
    public class CrystalLevelData : ScriptableObject
    {
        [SerializeField, LevelEditorSetting] List<CrystalStageData> stages;
        public List<CrystalStageData> Stages => stages;

        
        [SerializeField, HideInInspector, LevelEditorSetting] int invalidStageIndex; //Level editor cached validation
        [SerializeField, HideInInspector, LevelEditorSetting] int presentHolesCount; //Level editor cached validation
        [SerializeField, HideInInspector, LevelEditorSetting] int requiredHolesCount; //Level editor cached validation

        [SerializeField] bool useInRandomizer = true;
        public bool UseInRandomizer => useInRandomizer;

        [SerializeField] IntToggle overrideLevelReward;
        public IntToggle OverrideLevelReward => overrideLevelReward;

        [SerializeField] CrystalPUPrice[] powerUpsReward;
        public CrystalPUPrice[] PowerUpsReward => powerUpsReward;
    }
}