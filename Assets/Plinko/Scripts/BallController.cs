using General;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BallController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D thisRigidBody;
    [SerializeField] public Image ballImage;
    public float betVal;
    private bool isDestroy;
    public Ball ballType = Ball.Green;
    private GameplayScreen GPSCI => GameplayScreen.Inst;
    public GameObject itemPrev;

    private void Awake()
    {
        if (ballImage == null) ballImage = GetComponent<Image>();
        if (thisRigidBody == null) thisRigidBody = transform.GetComponent<Rigidbody2D>();
        thisRigidBody.simulated = true;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Is trigger " +  collision.gameObject.name);
    }
    public void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Ball Collider");
        if (isDestroy) return;
        Debug.Log("Ball Collision " + collision.gameObject.name);
        if (collision.gameObject.GetComponent<RewardMultiplier>())
        {
            // SoundManager.Inst.Play("Plinko");
            isDestroy = true;
            var mul = collision.gameObject.GetComponent<RewardMultiplier>();
            mul.ballAnim.Play();

            if (GPSCI.selectPlinko == Plinko.Single)
            {
                var aa = Instantiate(itemPrev, GameplayScreen.Inst.rewardParent.transform)
                    .GetComponent<HistoryController>();
                aa.SetData("", betVal.ToString(), mul.GetRewardValue().ToString(),
                    (betVal * mul.GetRewardValue()).ToString());
            }
            else GPSCI.GenerateLastReward(mul.GetRewardValue(), (int)ballType);

            // GeneralDataManager.Coins += (betVal * mul.GetRewardValue());
            Destroy(gameObject);
        }

        if (collision.gameObject.GetComponent<CircleController>())
        {
            //   SoundManager.Inst.Play("Hit");
            collision.gameObject.GetComponent<CircleController>().circleAnim.Play();
        }

        if (collision.gameObject.GetComponent<FinishLineController>()) Destroy(gameObject);
    }
}