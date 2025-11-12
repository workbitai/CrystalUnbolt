using CrystalUnbolt;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum Mode
{
    Low,
    Medium,
    High
}
public enum Rows
{
    Row11,
    Row12,
    Row13,
    Row14,
    Row15,
    Row16
}
public enum State
{
    Manual,
    Auto
}
public enum Ball
{
    Green,
    Yellow,
    Red
}
public enum Plinko
{
    Single,
    Double
}
public class GameplayScreen : MonoBehaviour
{
    public static GameplayScreen Inst;
         
    public Mode selectMode = Mode.Low;
    public Rows selectRow = Rows.Row11;
    public State selectState = State.Manual;
    public Ball selectBall = Ball.Green;
    public Plinko selectPlinko = Plinko.Single;
    [SerializeField] public GameObject  rewardParent, rewardObj;

    [SerializeField] public Button backBtn;
    [SerializeField] public List<GameObject> rowListObject = new List<GameObject>();

    public float betAmount = 1;
    public int ballsCount = 20;

    public Action OnSetScreenText;
    public Action OnChangeRows;

    private bool isMode, isRows;
    private static readonly int Close = Animator.StringToHash("Close");
    private static readonly int Open = Animator.StringToHash("Open");

    private bool isGenerate = false;

    [SerializeField] List<BallSpawner> ballSpawners = new List<BallSpawner>();

    public bool IsGenerateBall
    {
        get => isGenerate;
        set
        {
            isGenerate = value;
            OnSetScreenText?.Invoke();
        }
    }

    public float BetAmount
    {
        get => betAmount;
        private set
        {
            betAmount = value;
            OnSetScreenText?.Invoke();
        }
    }

    public int BallsCount
    {
        get => ballsCount;
        private set
        {
            ballsCount = value;
            OnSetScreenText?.Invoke();
        }
    }

    private void Awake()
    {
        if (Inst == null) Inst = this;
    }

    private void Update()
    {
        backBtn.interactable = CheckClickOnUi();
    }

    private void Start()
    {
        var active = SceneManager.GetActiveScene().name;
        selectPlinko = active.Contains("BounceBlitz") ? Plinko.Single : Plinko.Double;

        OnChangeRows += OnSetRows;
        OnSetScreenText?.Invoke();
        OnChangeRows?.Invoke();
    }

    private void OnDestroy()
    {
        OnChangeRows -= OnSetRows;
    }

    #region Buttons

   
    public void OnPlayBtnClick(int ballType)
    {
        if (selectPlinko == Plinko.Double && selectState == State.Auto && IsGenerateBall) return;

        if (selectState == State.Manual)
        {
                if (ballSpawners.Count > 0)
                {
                    foreach (var spawner in ballSpawners)
                    {
                        spawner.SpawnBall(ballType);
                    }
                }
           /* if (GeneralDataManager.CheckGemAvailable((BetAmount * (selectPlinko == Plinko.Single ? 1 : 2))))
            {
                GeneralDataManager.Coins -= (BetAmount * (selectPlinko == Plinko.Single ? 1 : 2));

                else GameManager.ShowMessage("", $"Ball spawner not found.");
                //if (BallSpawner.Inst != null) BallSpawner.Inst.SpawnBall();
            }
            else GameManager.ShowMessage("", $"You Don't have enough Coins!");*/
        }

        if (selectPlinko == Plinko.Single && selectState == State.Auto)
        {
            if (IsGenerateBall) IsGenerateBall = false;
            else
            {
                IsGenerateBall = true;
                if (ballSpawners.Count > 0)
                {
                    foreach (var spawner in ballSpawners)
                    {
                        spawner.StartCoroutine(spawner.GenerateAutoBall(Random.Range(0, 3)));
                    }
                }
              //  else GameManager.ShowMessage("", $"Ball spawner not found.");
            }
        }

        OnSetScreenText?.Invoke();
    }

    public void OnAutoModeBtnClick()
    {
        selectState = selectState == State.Auto ? State.Manual : State.Auto;
        if (IsGenerateBall) IsGenerateBall = false;
        if (selectState == State.Auto)
        {
            if (IsGenerateBall) IsGenerateBall = false;
            else
            {
                IsGenerateBall = true;
                if (ballSpawners.Count > 0)
                {
                    foreach (var spawner in ballSpawners)
                    {
                        spawner.StartCoroutine(spawner.GenerateAutoBall(Random.Range(0, 3)));
                    }
                }
             //   else GameManager.ShowMessage("", $"Ball spawner not found.");
            }
        }

        OnSetScreenText?.Invoke();
    }

    public void OnStateBtnClick(int index)
    {
        if (!CheckClickOnUi()) return;
        if (selectState == (State)index) return;
        if (IsGenerateBall) IsGenerateBall = false;
        selectState = (State)index;
        OnSetScreenText?.Invoke();
    }
    public void OnDivideBtnClick()
    {
        if (selectState == State.Manual)
        {
            if (BetAmount <= 1f) return;
            BetAmount /= 2;
            return;
        }

        if (BallsCount <= 10) return;
        BallsCount /= 2;
    }

    public void OnMinBtnClick()
    {
        BetAmount = 1;
    }

    #endregion
    public void ExitPopUpConfirmExitButton()
    {
        SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);

        CrystalLivesSystem.UnlockLife(true);

        ScreenOverlay.Show(0.3f, () =>
        {
          //  levelQuitPopUp.Hide();

            ScreenManager.DisableScreen<CrystalUIPause>();
            ScreenManager.DisableScreen<CrystalUIGame>();

            //ScreenManager.OnPopupWindowClosed(this);

            CrystalGameManager.ReturnToMenu();

            ScreenOverlay.Hide(0.3f);
        });
    }

    public static bool CheckClickOnUi()
    {
        var ballScreen = FindObjectsOfType<BallController>().ToList();
        return ballScreen.Count <= 0;
    }

    public void OnSetRows()
    {
        for (int i = 0; i < rowListObject.Count; i++)
        {
            rowListObject[i].SetActive(i == (int)selectRow);
        }
    }

    private int count = 0;

    public void GenerateLastReward(float reward, int ballType)
    {
        count++;
      /*  var rewardTile = Instantiate(rewardObj, rewardParent.transform).GetComponent<CollectReward>();
        rewardTile.transform.SetSiblingIndex(0);
        rewardTile.SetText($"{reward}x", ballType);
        rewardTile.name = $"{count}";*/

        if (rewardParent.transform.childCount > 20)
        {
            List<GameObject> list = new List<GameObject>();

            for (int i = 0; i < rewardParent.transform.childCount; i++)
            {
                if (i > 8) list.Add(rewardParent.transform.GetChild(i).gameObject);
            }

            foreach (var obj in list)
            {
                Destroy(obj);
            }
        }
    }
}