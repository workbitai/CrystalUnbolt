using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalUnbolt
{
    public class CrystalUIDailyBonus : BaseScreen, IPopupWindow
    {
        [Header("References")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private RectTransform chest;
        [SerializeField] private RectTransform claimButton;
        [SerializeField] private RectTransform adButton;
        [SerializeField] private Button closeButton, addCoin_WithAds;
        [SerializeField] private AudioClip lifeRecievedAudio;
        private SimpleBoolCallback panelClosed;

        public bool IsOpened => isPageDisplayed;

        public override void Init()
        {
            //  if (addCoin_WithAds != null) addCoin_WithAds.onClick.AddListener(OnButtonClick);
            //  closeButton.onClick.AddListener(OnCloseClicked);
        }
        [Header("Chest Buttons")]
        public Button safeChestButton, claim_btn;
        public Button riskChestButton;

        [Header("Chest Images")]
        public Image safeChestImage;
        public Image riskChestImage;

        [Header("Chest Sprites")]
        public Sprite closedChestSprite;
        public Sprite openChestSprite;

        [Header("Reward Texts")]
        public TMP_Text safeRewardText;
        public TMP_Text riskRewardText;

        [Header("Claim Button")]

        [Header("Rewards")]
        public int safeReward = 500;
        private int riskReward;
        private bool hasChosen = false;

        private void Start()
        {
            // Initial UI
            safeChestImage.sprite = closedChestSprite;
            riskChestImage.sprite = closedChestSprite;
            safeRewardText.text = "";
            riskRewardText.text = "";
            claimButton.gameObject.SetActive(false);

            // Assign button events
            safeChestButton.onClick.AddListener(() => OnChestClicked(true));
            riskChestButton.onClick.AddListener(() => OnChestClicked(false));
        }

        private void OnChestClicked(bool isSafeChest)
        {
            if (hasChosen) return;
            hasChosen = true;

            // Disable other chest
            safeChestButton.interactable = false;
            riskChestButton.interactable = false;

            // Animate chest open
            if (isSafeChest)
            {
                StartCoroutine(OpenChest(safeChestImage, safeRewardText, safeReward));
            }
            else
            {
                riskReward = Random.Range(0, 1001); // random 0–1000
                StartCoroutine(OpenChest(riskChestImage, riskRewardText, riskReward));
            }

            claim_btn.gameObject.SetActive(true);
            claim_btn.onClick.AddListener(ClaimReward);
        }

        private IEnumerator OpenChest(Image chestImage, TMP_Text rewardText, int rewardAmount)
        {
            // Simple bounce animation
            Vector3 originalScale = chestImage.transform.localScale;

            // Chest closes → slightly shrinks
            for (float t = 0; t < 1; t += Time.deltaTime * 6)
            {
                chestImage.transform.localScale = Vector3.Lerp(originalScale, originalScale * 0.9f, t);
                yield return null;
            }

            // Swap sprite to open chest
            chestImage.sprite = openChestSprite;

            // Bounce back
            for (float t = 0; t < 1; t += Time.deltaTime * 6)
            {
                chestImage.transform.localScale = Vector3.Lerp(originalScale * 0.9f, originalScale * 1.05f, t);
                yield return null;
            }

            // Back to normal size
            chestImage.transform.localScale = originalScale;

            // Show reward text after a short delay
            yield return new WaitForSeconds(0.2f);
            rewardText.text = rewardAmount + " Coins";
        }

        private void ClaimReward()
        {
            int reward = 0;
            if (!string.IsNullOrEmpty(safeRewardText.text))
                reward = safeReward;
            else if (!string.IsNullOrEmpty(riskRewardText.text))
                reward = riskReward;

            Debug.Log($"Claimed {reward} coins!");
            claim_btn.interactable = false;

            // Save daily claim (optional)
            PlayerPrefs.SetString("LastBonusDate", System.DateTime.Now.ToString("yyyyMMdd"));
        }

        public void OnButtonClick()
        {
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif
            MyAdsAdapter.ShowRewardBasedVideo(success =>
            {
                ScreenManager.CloseScreen<CrystalUIDailyBonus>();

                if (success)
                {
                    EconomyManager.Add(CurrencyType.Coins, 100);

                    if (lifeRecievedAudio != null)
                        SoundManager.PlaySound(lifeRecievedAudio);

                    panelClosed?.Invoke(true);
                }
            });
        }
        public override void PlayShowAnimation()
        {
            // Initial states
            chest.localScale = Vector3.zero;
            claimButton.localScale = Vector3.zero;
            adButton.localScale = Vector3.zero;
            backgroundImage.SetAlpha(0);

            // Background fade
            backgroundImage.DOFade(0.4f, 0.3f);

            // Animate chest
            chest.DOScale(1.1f, 0.4f).SetEasing(Ease.Type.BackOut)
                .OnComplete(() => chest.DOScale(1f, 0.1f));

            // Time bar slide down
            //timeBar.DOAnchorPosY(0f, 0.4f).SetEasing(Ease.Type.SineOut).SetDelay(0.1f);

            // Claim button
            claimButton.DOScale(1.05f, 0.3f).SetEasing(Ease.Type.BackOut).SetDelay(0.3f)
                .OnComplete(() => claimButton.DOScale(1f, 0.1f));

            // Ad button
            adButton.DOScale(1.05f, 0.3f).SetEasing(Ease.Type.BackOut).SetDelay(0.4f)
                .OnComplete(() => adButton.DOScale(1f, 0.1f));

            ScreenManager.OnPageOpened(this);
            ScreenManager.OnPopupWindowOpened(this);
        }

        public override void PlayHideAnimation()
        {
            chest.DOScale(0, 0.3f).SetEasing(Ease.Type.BackIn);
            claimButton.DOScale(0, 0.3f).SetEasing(Ease.Type.BackIn);
            adButton.DOScale(0, 0.3f).SetEasing(Ease.Type.BackIn);
            //  timeBar.DOAnchorPosY(300f, 0.3f).SetEasing(Ease.Type.SineIn);
            backgroundImage.DOFade(0, 0.3f);

            ScreenManager.OnPageClosed(this);
            ScreenManager.OnPopupWindowClosed(this);
        }

        private void OnCloseClicked()
        {
            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif
            ScreenManager.CloseScreen<CrystalUIDailyBonus>();
        }

        public override void PlayShowAnimationMainReturn() { }
    }
}
