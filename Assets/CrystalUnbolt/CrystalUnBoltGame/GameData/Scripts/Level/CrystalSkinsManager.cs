using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrystalUnbolt
{
    public class CrystalSkinsManager
    {
        public CrystalPlanksSkinData Data { get; private set; }
        private bool IsSkinLoaded => Data != null;

        private Dictionary<CrystalPlankType, PoolGeneric<CrystalPlankController>> plankPools;
        private PoolGeneric<CrystalScrewController> screwPool;
        private PoolGeneric<CrystalBaseHole> baseHolePool;
        private PoolGeneric<CrystalPlankHole> plankHolePool;
        private PoolGeneric<CrystalBaseController> baseBoardPool;

        public void LoadSkin(CrystalPlanksSkinData data)
        {
            Data = data;

            plankPools = new Dictionary<CrystalPlankType, PoolGeneric<CrystalPlankController>>();

            for(int i = 0; i < data.Planks.Count; i++)
            {
                CrystalPlankData CrystalPlankData = data.Planks[i];

                PoolGeneric<CrystalPlankController> plankPool = new PoolGeneric<CrystalPlankController>(CrystalPlankData.Prefab, $"{CrystalPlankData.Type}");

                plankPool.Init();

                plankPools.Add(CrystalPlankData.Type, plankPool);
            }

            screwPool = new PoolGeneric<CrystalScrewController>(data.ScrewPrefab, "Screw");
            screwPool.Init();

            plankHolePool = new PoolGeneric<CrystalPlankHole>(data.PlankHolePrefab, "Plank Hole");
            plankHolePool.Init();

            baseHolePool = new PoolGeneric<CrystalBaseHole>(data.BaseHolePrefab, "Base Hole");
            baseHolePool.Init();

            baseBoardPool = new PoolGeneric<CrystalBaseController>(data.BackPrefab, "Base Board");
            baseBoardPool.Init();
        }

        public CrystalPlankController GetPlank(CrystalPlankType CrystalPlankType)
        {
            if (!IsSkinLoaded) return null;

            if(!plankPools.ContainsKey(CrystalPlankType)) return null;

            return plankPools[CrystalPlankType].GetPooledComponent();
        }

        public CrystalScrewController GetScrew()
        {
            if(!IsSkinLoaded) return null;

            return screwPool.GetPooledComponent();
        }

        public CrystalBaseHole GetBaseHole()
        {
            if(!IsSkinLoaded) return null;

            return baseHolePool.GetPooledComponent();
        }

        public CrystalPlankHole GetPlankHole()
        {
            if (!IsSkinLoaded) return null;

            return plankHolePool.GetPooledComponent();
        }

        public CrystalBaseController GetBase()
        {
            if(!IsSkinLoaded) return null;

            return baseBoardPool.GetPooledComponent();
        }

        public Color GetLayerPlankColor(int layerId)
        {
            return Data.GetLayerColor(layerId);
        }

        public void UnloadSkin()
        {
            if(!IsSkinLoaded) return;

            ObjectPoolManager.DestroyPool(plankHolePool);
            ObjectPoolManager.DestroyPool(baseHolePool);
            ObjectPoolManager.DestroyPool(screwPool);
            ObjectPoolManager.DestroyPool(baseBoardPool);

            foreach(PoolGeneric<CrystalPlankController> plankPool in plankPools.Values)
            {
                ObjectPoolManager.DestroyPool(plankPool);
            }

            Data = null;
        }
    }
}