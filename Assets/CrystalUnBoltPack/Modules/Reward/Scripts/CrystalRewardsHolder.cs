using UnityEngine;
using UnityEngine.Events;

namespace CrystalUnbolt
{
    public abstract class CrystalRewardsHolder : MonoBehaviour
    {
        [Group("Events")]
        [SerializeField] UnityEvent rewardReceived;
        public UnityEvent RewardReceived => rewardReceived;

        protected CrystalReward[] rewards;

        protected void InitializeComponents()
        {
            // Cache all assigned CrystalReward components
            // Note: Custom editor hides these components in Inspector, but they are still there (you can see them by turing Debug mode in Inspector window)
            rewards = GetComponents<CrystalReward>();

            // Initialize rewards
            for (int i = 0; i < rewards.Length; i++)
            {
                rewards[i].Init();
            }
        }

        protected void ApplyRewards()
        {
            for (int i = 0; i < rewards.Length; i++)
            {
                rewards[i].ApplyReward();
            }

            // Invoke purhase event
            rewardReceived?.Invoke();
        }
    }
}
