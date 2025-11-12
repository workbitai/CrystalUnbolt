using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CrystalUnbolt
{
    public class CrystalPURewardUIBehavior : MonoBehaviour
    {
        [SerializeField] Image iconImage;
        [SerializeField] TextMeshProUGUI amountText;
        [SerializeField] string amountFormat = "x{0}";

        public void Initialise(CrystalPUPrice price)
        {
            CrystalPUBehavior behavior = CrystalPUController.GetPowerUpBehavior(price.PowerUpType);
            iconImage.sprite = behavior.Settings.Icon;
            amountText.text = string.IsNullOrEmpty(amountFormat) ? price.Amount.ToString() : string.Format(amountFormat, price.Amount);
        }
    }
}
