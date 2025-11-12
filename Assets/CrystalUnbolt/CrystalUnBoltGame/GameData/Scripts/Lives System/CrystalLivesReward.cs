using TMPro;
using UnityEngine;

namespace CrystalUnbolt
{
    public class CrystalLivesReward : CrystalReward
    {
        [SerializeField] int livesAmount = 1;

        [Space]
        [SerializeField] TextMeshProUGUI amountText;

        public override void Init()
        {
            if (amountText != null)
            {
                amountText.text = livesAmount.ToString();
            }
        }

        public override void ApplyReward()
        {
            CrystalLivesSystem.AddLife(livesAmount, true);
        }
    }
}
