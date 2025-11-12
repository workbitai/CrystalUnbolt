using UnityEngine;

namespace CrystalUnbolt
{
    [System.Serializable]
    public class CrystalPURewardPanel
    {
        [SerializeField] RectTransform containerTransform;
        [SerializeField] GameObject powerUpPrefab;

        private Pool panelsPool;

        public void Init()
        {
            panelsPool = new Pool(powerUpPrefab, containerTransform);
        }

        public void Unload()
        {
            ObjectPoolManager.DestroyPool(panelsPool);
        }
        
        public void Show(CrystalPUPrice[] rewards, float baseDelay)
        {
            if (rewards.IsNullOrEmpty())
            {
                containerTransform.gameObject.SetActive(false);

                return;
            }

            containerTransform.gameObject.SetActive(true);
            panelsPool.ReturnToPoolEverything();

            for(int i = 0; i < rewards.Length; i++)
            {
                GameObject uiElement = panelsPool.GetPooledObject();
                uiElement.transform.SetAsLastSibling();

                CrystalPURewardUIBehavior rewardUIBehavior = uiElement.GetComponent<CrystalPURewardUIBehavior>();
                rewardUIBehavior.Initialise(rewards[i]);

                uiElement.transform.localScale = Vector3.zero;
                uiElement.transform.DOScale(Vector3.one, 0.24f, baseDelay + 0.1f * i).SetEasing(Ease.Type.CubicOut);
            }
        }
    }
}
