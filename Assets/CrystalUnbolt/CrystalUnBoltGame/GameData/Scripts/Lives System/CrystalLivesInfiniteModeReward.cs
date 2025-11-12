using TMPro;
using UnityEngine;

namespace CrystalUnbolt
{
    public class CrystalLivesInfiniteModeReward : CrystalReward
    {
        [SerializeField] float durationInMinutes = 60;

        [Space]
        [SerializeField] TextMeshProUGUI durationText;
        [SerializeField] string durationFormat = "{hh}hrs";

        public override void Init()
        {
            if (durationText != null)
            {
                durationText.text = TimeUtils.GetFormatedTime(durationInMinutes, durationFormat);
            }
        }

        public override void ApplyReward()
        {
            CrystalLivesSystem.EnableInfiniteMode(durationInMinutes * 60);
        }
    }
}
