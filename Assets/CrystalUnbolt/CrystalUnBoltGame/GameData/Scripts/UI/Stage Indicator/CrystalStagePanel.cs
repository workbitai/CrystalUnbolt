using UnityEngine;

namespace CrystalUnbolt
{
    [System.Serializable]
    public class CrystalStagePanel
    {
        [SerializeField] Transform containerTransform;
        [SerializeField] GameObject stagePrefab;
        [SerializeField] GameObject spacerPrefab;

        private Pool stagePool;
        private Pool spacerPool;

        private CrystalStageBehavior[] stages;

        public void Init()
        {
            if (stagePrefab == null)
            {
                Debug.LogError("[CrystalStagePanel] Stage Prefab is null! Cannot initialize stage pool.");
                return;
            }
            
            if (spacerPrefab == null)
            {
                Debug.LogError("[CrystalStagePanel] Spacer Prefab is null! Cannot initialize spacer pool.");
                return;
            }
            
            if (containerTransform == null)
            {
                Debug.LogError("[CrystalStagePanel] Container Transform is null!");
                return;
            }
            
            stagePool = new Pool(stagePrefab, containerTransform);
            spacerPool = new Pool(spacerPrefab, containerTransform);
        }

        public void Spawn(int amount)
        {
            Clear();

            stages = new CrystalStageBehavior[amount];
            for (int i = 0; i < amount; i++)
            {
                GameObject stageObject = stagePool.GetPooledObject();
                stageObject.transform.SetAsLastSibling();

                CrystalStageBehavior stageBehavior = stageObject.GetComponent<CrystalStageBehavior>();
                stageBehavior.SetDefaultColor();

                stages[i] = stageBehavior;

                if (i + 1 < amount)
                {
                    GameObject spacerObject = spacerPool.GetPooledObject();
                    spacerObject.transform.SetAsLastSibling();
                }
            }

            if (amount == 1)
                stages[0].gameObject.SetActive(false);
        }

        public void Activate(int index)
        {
            if(stages.IsInRange(index))
            {
                stages[index].SetActiveColor();
            }
        }

        public void Clear()
        {
            stagePool.ReturnToPoolEverything();
            spacerPool.ReturnToPoolEverything();
        }

        public void Unload()
        {
            if (stagePool != null)
                ObjectPoolManager.DestroyPool(stagePool);
            if (spacerPool != null)
                ObjectPoolManager.DestroyPool(spacerPool);
        }
    }
}
