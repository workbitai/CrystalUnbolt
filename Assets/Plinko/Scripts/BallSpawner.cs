
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    public static BallSpawner Inst;
    private GameplayScreen GPSCI => GameplayScreen.Inst;

    [SerializeField] public GameObject ballObj;
    [SerializeField] private RectTransform topBar;
    [SerializeField] private Sprite greenSp, yellowSp, redSp;

    private void Awake()
    {
        if (Inst == null) Inst = this;
    }

    public int minX = 8, maxX = 8;

    public void SpawnBall(int ballT)
    {
        var ball = Instantiate(ballObj, transform).GetComponent<RectTransform>();
        Debug.Log("Ball Position " + ball.GetComponent<RectTransform>().transform.position);
        Physics2D.SyncTransforms();
        // Get width of top bar in local coordinates
        float halfWidth = topBar.rect.width / 2f;

        // Random X position within top bar width
        float xPos = Random.Range(-halfWidth, halfWidth);

        ball.GetComponent<BallController>().ballType = (Ball)ballT;
        ball.GetComponent<BallController>().betVal = GPSCI.BetAmount;

        if (GPSCI.selectPlinko == Plinko.Double)
        {
            ball.GetComponent<BallController>().ballImage.sprite = ballT switch
            {
                0 => greenSp,
                1 => yellowSp,
                2 => redSp,
                _ => ball.GetComponent<SpriteRenderer>().sprite
            };
        }

        // Set position relative to top bar
        Vector2 spawnPos = new Vector2(xPos, topBar.anchoredPosition.y - 30f); // small offset below the bar
        ball.anchoredPosition = spawnPos;
    }
    public IEnumerator GenerateAutoBall(int ballT)
    {
        var ballCount = GPSCI.BallsCount;
        while (GPSCI.IsGenerateBall)
        {
            if (ballCount <= 0)
            {
                GPSCI.IsGenerateBall = false;
                GPSCI.selectState = State.Manual;
                yield break;
            }

            // if (GeneralDataManager.CheckGemAvailable(GPSCI.BetAmount)) GeneralDataManager.Coins -= GPSCI.BetAmount;
            else GPSCI.IsGenerateBall = false;

            SpawnBall(Random.Range(0, 3));
            yield return new WaitForSeconds(1f);

            if (!GPSCI.IsGenerateBall) ballCount--;
            Debug.Log(ballCount);
        }
    }
}