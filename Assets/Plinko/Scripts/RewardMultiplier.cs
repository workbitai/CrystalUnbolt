using System;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class RewardMultiplier : MonoBehaviour
{
    [SerializeField] public Text mulText;
    [SerializeField] public Animation ballAnim;
    public Ball multiType = Ball.Green;
    public float multiplierValue = 0.1f;//, lowMultiVal = 1f, highMulVal = 0.5f;
    GameplayScreen GPSCI => GameplayScreen.Inst;

    private IEnumerator Start()
    {
        while (!GPSCI)
        {
            yield return new WaitForEndOfFrame();
        }

        GameplayScreen.Inst.OnSetScreenText += SetMultiplierText;
        SetMultiplierText();
    }

    private void OnDestroy()
    {
        GameplayScreen.Inst.OnSetScreenText -= SetMultiplierText;
    }

    public float GetRewardValue()
    {
        if (GPSCI.selectPlinko == Plinko.Single)
        {
            return GPSCI.selectMode switch
            {
             //   Mode.Low => lowMultiVal,
                Mode.Medium => multiplierValue,
             //   Mode.High => highMulVal,
             //   _ => lowMultiVal
            };
        }
        else
        {
            return multiType switch
            {
             //   Ball.Green => lowMultiVal,
                Ball.Yellow => multiplierValue,
             //   Ball.Red => highMulVal,
             //   _ => lowMultiVal
            };
        }
    }

    public void SetMultiplierText()
    {
        if (GPSCI.selectPlinko == Plinko.Single)
        {
            mulText.text = GPSCI.selectMode switch
            {
             //   Mode.Low => $"{lowMultiVal}x",
                Mode.Medium => $"{multiplierValue}",
              //  Mode.High => $"{highMulVal}x",
                _ => mulText.text
            };
        }
        else
        {
            mulText.text = multiType switch
            {
             //   Ball.Green => $"{lowMultiVal}x",
                Ball.Yellow => $"{multiplierValue}",
             //   Ball.Red => $"{highMulVal}x",
                _ => mulText.text
            };
        }
    }
}