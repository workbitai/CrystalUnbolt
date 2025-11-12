using System;
using UnityEngine;
using UnityEngine.UI;

namespace General
{
    public class HistoryController : MonoBehaviour
    {
        [SerializeField] private Text timeText, betText, payoutText, profitText;

        public void SetData(string time, string bet, string payout, string profit)
        {
            Set_Remaining_Time(timeText);
            betText.text = bet;
            payoutText.text = payout;
            profitText.text = profit;
        }

        private static void Set_Remaining_Time(Text text)
        {
            DateTime now = DateTime.Now;
            DateTime startOfDay = now.Date;
            TimeSpan timeSpan = now - startOfDay;
            var time = $"{timeSpan.Hours:D2} : {timeSpan.Minutes:D2} : {timeSpan.Seconds:D2}";
            text.text = time;
        }
    }
}