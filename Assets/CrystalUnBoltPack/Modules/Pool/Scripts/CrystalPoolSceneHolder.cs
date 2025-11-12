using UnityEngine;

namespace CrystalUnbolt
{
    [DefaultExecutionOrder(-5)]
    public class CrystalPoolSceneHolder : MonoBehaviour
    {
        [SerializeField] Pool[] pools;

        private void Awake()
        {
            foreach(Pool pool in pools)
            {
                pool.Init();
            }
        }

        private void OnDestroy()
        {
            foreach (Pool pool in pools)
            {
                ObjectPoolManager.DestroyPool(pool);
            }
        }
    }
}