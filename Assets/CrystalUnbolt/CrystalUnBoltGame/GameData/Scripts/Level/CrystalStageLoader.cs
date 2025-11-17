using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace CrystalUnbolt
{
    public class CrystalStageLoader
    {
        public List<CrystalBaseHole> BaseHoles { get; private set; }
        public List<CrystalPlankController> Planks { get; private set; }
        public List<CrystalScrewController> Screws { get; private set; }

        private CrystalBaseController baseBoard;

        public int AmountOfPlanks => Planks.Count;

        public bool StageLoaded { get; private set; }

        private CrystalSkinsManager CrystalSkinsManager;

        public CrystalStageLoader(CrystalSkinsManager CrystalSkinsManager)
        {
            this.CrystalSkinsManager = CrystalSkinsManager;
        }

        public void LoadStage(CrystalStageData CrystalLevelData)
        {
            float fixedDeltaTile = Time.fixedDeltaTime;
            Time.fixedDeltaTime = 10;
            CrystalGameOverArranger.instance.ResetState();
            BaseHoles = new List<CrystalBaseHole>();
            Planks = new List<CrystalPlankController>();
            Screws = new List<CrystalScrewController>();

            for (int i = 0; i < CrystalLevelData.HolePositions.Count; i++)
            {
                CrystalHoleData CrystalHoleData = CrystalLevelData.HolePositions[i];

                CrystalBaseHole baseHole = CrystalSkinsManager.GetBaseHole();
                baseHole.ResetNewData();
                baseHole.Init(CrystalHoleData);
                BaseHoles.Add(baseHole);
            }

            CrystalLevelData.PlanksData.Sort((data1, data2) => data1.PlankLayer - data2.PlankLayer);

            for (int i = 0; i < CrystalLevelData.PlanksData.Count; i++)
            {
                CrystalPlankLevelData CrystalPlankLevelData = CrystalLevelData.PlanksData[i];

                CrystalPlankController plank = CrystalSkinsManager.GetPlank(CrystalPlankLevelData.PlankType);

                Color plankColor = CrystalSkinsManager.GetLayerPlankColor(CrystalPlankLevelData.PlankLayer);

                plank.Init(CrystalPlankLevelData, plankColor, i);
                plank.SetHoles(CrystalPlankLevelData.ScrewsPositions, CrystalSkinsManager);
                Planks.Add(plank);
            }

            for (int i = 0; i < Planks.Count; i++)
            {
                CrystalPlankController firstPlank = Planks[i];
                for (int j = i + 1; j < Planks.Count; j++)
                {
                    CrystalPlankController secondPlank = Planks[j];

                    if (firstPlank.Layer != secondPlank.Layer)
                    {
                        firstPlank.IgnorePlank(secondPlank);
                    }
                    else
                    {
                        firstPlank.CollideWithPlank(secondPlank);
                    }
                }
            }
            for (int i = 0; i < BaseHoles.Count; i++)
            {
                Debug.Log("I  =>  "+i);
                BaseHoles[i].ResetDataHole();

            }
            ApplyLockedHoleRules(CrystalLevelData);
            for (int i = 0; i < CrystalLevelData.HolePositions.Count; i++)
            {
                CrystalHoleData CrystalHoleData = CrystalLevelData.HolePositions[i];

                if (CrystalHoleData.HasScrew)
                {
                    CrystalScrewController screw = CrystalSkinsManager.GetScrew();
                    screw.ResetNewData();
                    screw.transform.position = CrystalHoleData.Position.SetZ(0);
                    screw.Init(BaseHoles, Planks);

                    Screws.Add(screw);
                }
            }

            for (int i = 0; i < Planks.Count; i++)
            {
                CrystalPlankController plank = Planks[i];

                plank.EnableColliders();
            }

            baseBoard = CrystalSkinsManager.GetBase();
            baseBoard.transform.position = Vector3.forward;

            Tween.DelayedCall(0.2f, () =>
            {
                Time.fixedDeltaTime = fixedDeltaTile;

                for (int i = 0; i < Planks.Count; i++)
                {
                    Planks[i].StartSimulation();
                }

                for (int i = 0; i < Screws.Count; i++)
                {
                    Screws[i].EnableCollider();
                }
            });

            StageLoaded = true;

            CrystalUIGame gameUI = ScreenManager.GetPage<CrystalUIGame>();
            gameUI.PowerUpsUIController.OnStageStarted();
        }

        private void ApplyLockedHoleRules(CrystalStageData stageData)
        {
            Debug.Log("LockedHole Rules --------- ");
            if (stageData == null || stageData.LockedHoles.IsNullOrEmpty())
                return;

            for (int i = 0; i < stageData.LockedHoles.Count; i++)
            {
                CrystalLockedHoleConfig config = stageData.LockedHoles[i];
                if (config == null || !config.Enabled)
                    continue;

                if (config.HoleIndex < 0 || config.HoleIndex >= BaseHoles.Count)
                {
                    Debug.LogWarning($"[StageLoader] Locked hole index '{config.HoleIndex}' is out of range for stage '{stageData}'. Skipping.");
                    continue;
                }

                BaseHoles[config.HoleIndex].ApplyLock(config);
            }
        }

        public void PlaceAdditionalBaseHole(Vector2 position)
        {
            CrystalHoleData CrystalHoleData = new CrystalHoleData(position, false);

            CrystalBaseHole baseHole = CrystalSkinsManager.GetBaseHole();
            baseHole.ResetDataHole();
            baseHole.Init(CrystalHoleData);
            BaseHoles.Add(baseHole);
        }

        public void UnloadStage(bool withParticle)
        {
            if (!StageLoaded) return;

            for (int i = 0; i < Planks.Count; i++)
            {
                if (Planks[i] != null && Planks[i].gameObject.activeSelf) Planks[i].Discard(withParticle);
            }
            for (int i = 0; i < BaseHoles.Count; i++)
            {
                if (BaseHoles[i] != null) BaseHoles[i].Discard();
                BaseHoles[i].ResetDataHole();
                BaseHoles[i].SetActive(false);
            }

            if (CrystalScrewController.SelectedScrew != null) CrystalScrewController.SelectedScrew.Deselect();
            for (int i = 0; i < Screws.Count; i++)
            {
                if (Screws[i] != null) Screws[i].Discard();
                Screws[i].ResetNewData();
               
                Screws[i].gameObject.SetActive(false);
            }
            CrystalGameOverArranger.instance.ResetState();
            baseBoard.Discard();

            Planks.Clear();
            BaseHoles.Clear();
            Screws.Clear();

            StageLoaded = false;
        }
    }
}
