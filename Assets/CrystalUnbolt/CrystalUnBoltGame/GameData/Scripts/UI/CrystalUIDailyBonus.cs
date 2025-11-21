using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalUnbolt
{
    public class CrystalUIDailyBonus : BaseScreen, IPopupWindow
    {
        private const string PREF_LAST_BONUS_TICKS = "DailyBonus_LastClaimTicks";
        private const int COOLDOWN_HOURS = 24;

        [Header("References")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private RectTransform chest;
        [SerializeField] private RectTransform claimButton;
        [SerializeField] private RectTransform adButton;
        [SerializeField] private Button closeButton, addCoin_WithAds;
        [SerializeField] private AudioClip lifeRecievedAudio, openChestAudioClip;
        private SimpleBoolCallback panelClosed;

        [Header("Timer")]
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text timerLabelText; // "NEXT REWARD AVAILABLE IN"

        [Header("Currency Animation")]
        [SerializeField] private RectTransform currencyTargetPanel; // Currency display panel (coins UI at top left)
        [SerializeField] private int coinAnimationCount = 10; // Number of coins to spawn

        [Header("Chest Buttons")]
        public Button safeChestButton, claim_btn;
        public Button riskChestButton;

        [Header("Chest Images")]
        public Image safeChestImage;
        public Image riskChestImage;

        [Header("Chest Sprites")]
        public Sprite closedChestSprite;
        public Sprite openChestSprite;

        [Header("Reward Display")]
        public TMP_Text rewardAmountText; // Single UI text to show the reward amount
        public Image rewardCoinIcon; // Coin icon next to reward text (optional)
        public GameObject rewardDisplayPanel; // Parent panel/object containing reward display (to hide entire panel)

        [Header("Bottom Text")]
        [SerializeField] private TMP_Text bottomInstructionText; // Text at bottom (e.g., "You can only pick one chest!")

        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem chestClickParticles; // <-- Assign particle system here

        [Header("Rewards")]
        [SerializeField] private int safeReward = 500;
        [SerializeField] private int riskRewardMin = 0;
        [SerializeField] private int riskRewardMax = 2000;

        [Header("Chest Scale")]
        [SerializeField][Range(0.5f, 1.0f)] private float chestScale = 0.8f; // Scale multiplier for chests (0.8 = 80% size)

        private int riskReward;
        private int chosenReward; // Store the chosen reward amount
        private Image chosenChestImage; // Store the chosen chest image for coin animation
        private bool hasChosen = false;
        private bool isRewardAvailable = false;
        private DateTime nextAvailableTime;
        private Coroutine timerCoroutine;

        public bool IsOpened => isPageDisplayed;
        private bool isInitialized = false;

        public override void Init()
        {
            if (isInitialized) return;
            isInitialized = true;

            // Remove existing listeners to prevent duplicates
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(OnCloseClicked);
            }

            if (addCoin_WithAds != null)
            {
                addCoin_WithAds.onClick.RemoveAllListeners();
                addCoin_WithAds.onClick.AddListener(OnButtonClick);
            }

            InitializeDailyBonus();

            // Start timer after initialization
            if (timerCoroutine == null)
                timerCoroutine = StartCoroutine(UpdateTimer());
        }

        private void OnEnable()
        {
            // Only start timer if already initialized
            if (isInitialized && timerCoroutine == null)
                timerCoroutine = StartCoroutine(UpdateTimer());
        }

        private void OnDisable()
        {
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                timerCoroutine = null;
            }
        }

        private void InitializeDailyBonus()
        {
            // Check if bonus is available
            CheckBonusAvailability();

            // Initialize UI state
            ResetUI();

            // Setup button listeners (remove existing first to prevent duplicates)
            if (safeChestButton != null)
            {
                safeChestButton.onClick.RemoveAllListeners();
                safeChestButton.onClick.AddListener(() => OnChestClicked(true));
            }

            if (riskChestButton != null)
            {
                riskChestButton.onClick.RemoveAllListeners();
                riskChestButton.onClick.AddListener(() => OnChestClicked(false));
            }

            if (claim_btn != null)
            {
                claim_btn.onClick.RemoveAllListeners();
                claim_btn.onClick.AddListener(OnClaimClicked);
                claim_btn.gameObject.SetActive(false);
            }

            // Hide reward display initially (especially after claiming)
            HideRewardDisplay();

            // Ensure particles are off initially
            TurnOffParticles();

            // Enable/disable chests based on availability
            SetChestsInteractable(isRewardAvailable);

            // Update bottom instruction text based on availability
            UpdateBottomInstructionText();
        }

        private void CheckBonusAvailability()
        {
            if (!PlayerPrefs.HasKey(PREF_LAST_BONUS_TICKS))
            {
                // First time - bonus is available
                isRewardAvailable = true;
                nextAvailableTime = DateTime.Now;
            }
            else
            {
                long ticks = Convert.ToInt64(PlayerPrefs.GetString(PREF_LAST_BONUS_TICKS, DateTime.Now.Ticks.ToString()));
                DateTime lastClaimTime = new DateTime(ticks);
                nextAvailableTime = lastClaimTime.AddHours(COOLDOWN_HOURS);

                TimeSpan timeRemaining = nextAvailableTime - DateTime.Now;
                isRewardAvailable = timeRemaining.TotalSeconds <= 0;
            }
        }

        private void ResetUI()
        {
            hasChosen = false;
            chosenReward = 0;
            chosenChestImage = null;

            // Reset chest images and scale
            Vector3 chestTargetScale = Vector3.one * chestScale;

            if (safeChestImage != null)
            {
                if (closedChestSprite != null)
                    safeChestImage.sprite = closedChestSprite;
                safeChestImage.SetNativeSize();
                safeChestImage.transform.localScale = chestTargetScale;
            }

            if (riskChestImage != null)
            {
                if (closedChestSprite != null)
                    riskChestImage.sprite = closedChestSprite;
                riskChestImage.SetNativeSize();
                riskChestImage.transform.localScale = chestTargetScale;
            }

            // Hide reward display
            HideRewardDisplay();

            // Turn off particles
            TurnOffParticles();

            // Hide claim button initially
            if (claim_btn != null)
            {
                claim_btn.gameObject.SetActive(false);
                claim_btn.interactable = true;
            }
        }

        private void SetChestsInteractable(bool interactable)
        {
            if (safeChestButton != null)
                safeChestButton.interactable = interactable;

            if (riskChestButton != null)
                riskChestButton.interactable = interactable;
        }

        private void HideRewardDisplay()
        {
            // Hide the entire reward display panel if assigned
            if (rewardDisplayPanel != null)
            {
                rewardDisplayPanel.SetActive(false);
            }
            else
            {
                // Fallback: hide individual elements if panel not assigned
                if (rewardAmountText != null)
                {
                    rewardAmountText.text = "";
                    rewardAmountText.gameObject.SetActive(false);
                }

                if (rewardCoinIcon != null)
                    rewardCoinIcon.gameObject.SetActive(false);
            }
        }

        private IEnumerator UpdateTimer()
        {
            while (true)
            {
                UpdateTimerDisplay();
                yield return new WaitForSeconds(1f);
            }
        }

        private void UpdateTimerDisplay()
        {
            if (timerText == null) return;

            try
            {
                if (isRewardAvailable)
                {
                    if (timerLabelText != null)
                        timerLabelText.text = "REWARD AVAILABLE NOW!";

                    timerText.text = "00:00:00";
                    UpdateBottomInstructionText(); // Update text when reward becomes available
                    return;
                }

                TimeSpan timeRemaining = nextAvailableTime - DateTime.Now;

                if (timeRemaining.TotalSeconds <= 0)
                {
                    isRewardAvailable = true;
                    SetChestsInteractable(true);
                    if (timerLabelText != null)
                        timerLabelText.text = "REWARD AVAILABLE NOW!";
                    timerText.text = "00:00:00";
                    UpdateBottomInstructionText(); // Update text when reward becomes available
                    return;
                }

                // Format as HH:MM:SS
                int hours = (int)timeRemaining.TotalHours;
                int minutes = timeRemaining.Minutes;
                int seconds = timeRemaining.Seconds;

                timerText.text = $"{hours:00}:{minutes:00}:{seconds:00}";

                if (timerLabelText != null)
                    timerLabelText.text = "NEXT REWARD AVAILABLE IN";
            }
            catch (Exception e)
            {
                Debug.LogError($"[DailyBonus] Error updating timer: {e.Message}");
            }
        }

        private void UpdateBottomInstructionText()
        {
            if (bottomInstructionText == null) return;

            if (isRewardAvailable)
            {
                bottomInstructionText.text = "You can only pick one chest!";
            }
            else
            {
                bottomInstructionText.text = "Come back tomorrow for your next reward!";
            }
        }

        private void OnChestClicked(bool isSafeChest)
        {
            if (hasChosen || !isRewardAvailable) return;

            if (openChestAudioClip != null)
                SoundManager.PlaySound(openChestAudioClip);
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif

            hasChosen = true;

            // Calculate risk reward if risk chest was chosen
            if (!isSafeChest)
            {
                riskReward = UnityEngine.Random.Range(riskRewardMin, riskRewardMax + 1);
            }

            // Show particles behind chosen chest position
            Image selectedChestImage = isSafeChest ? safeChestImage : riskChestImage;
            ShowParticles(selectedChestImage);

            // Animate chest opening
            StartCoroutine(OpenChest(isSafeChest));
        }

        private void ShowParticles(Image targetChestImage)
        {
            if (chestClickParticles == null || targetChestImage == null) return;

            RectTransform chestRect = targetChestImage.rectTransform;
            if (chestRect != null)
            {
                chestClickParticles.transform.position = chestRect.position;

                // If particles are also under the same canvas and have RectTransform,
                // keep them behind the chest in hierarchy
                RectTransform psRect = chestClickParticles.GetComponent<RectTransform>();
                if (psRect != null)
                {
                    int chestIndex = chestRect.GetSiblingIndex();
                    if (chestIndex > 0) psRect.SetSiblingIndex(chestIndex - 1);
                    else psRect.SetAsFirstSibling();
                }
            }

            chestClickParticles.gameObject.SetActive(true);
            chestClickParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            chestClickParticles.Play(true);
        }

        private void TurnOffParticles()
        {
            if (chestClickParticles == null) return;

            chestClickParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            chestClickParticles.gameObject.SetActive(false);
        }

        private IEnumerator OpenChest(bool isSafeChest)
        {
            Image chestImage = isSafeChest ? safeChestImage : riskChestImage;
            int rewardAmount = isSafeChest ? safeReward : riskReward;

            if (chestImage == null) yield break;

            chosenReward = rewardAmount;
            chosenChestImage = chestImage;

            Vector3 baseScale = Vector3.one * chestScale;
            Vector3 originalScale = chestImage.transform.localScale;

            float duration = 0.15f;
            float elapsed = 0f;
            Vector3 shrinkScale = baseScale * 0.9f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                chestImage.transform.localScale = Vector3.Lerp(originalScale, shrinkScale, t);
                yield return null;
            }

            if (openChestSprite != null)
            {
                chestImage.sprite = openChestSprite;
                chestImage.SetNativeSize();
            }

            Vector3 targetScale = baseScale;

            elapsed = 0f;
            Vector3 bounceScale = targetScale * 1.1f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                chestImage.transform.localScale = Vector3.Lerp(shrinkScale, bounceScale, t);
                yield return null;
            }

            chestImage.transform.DOScale(targetScale, 0.2f).SetEasing(Ease.Type.BackOut);
            yield return new WaitForSeconds(0.2f);

            chestImage.transform.localScale = targetScale;

            yield return new WaitForSeconds(0.1f);

            if (rewardDisplayPanel != null)
            {
                rewardDisplayPanel.SetActive(true);
                rewardDisplayPanel.transform.localScale = Vector3.zero;
                rewardDisplayPanel.transform.DOScale(1f, 0.3f).SetEasing(Ease.Type.BackOut);
            }

            if (rewardAmountText != null)
            {
                rewardAmountText.gameObject.SetActive(true);
                rewardAmountText.text = rewardAmount.ToString();

                if (rewardDisplayPanel == null)
                {
                    rewardAmountText.transform.localScale = Vector3.zero;
                    rewardAmountText.transform.DOScale(1f, 0.3f).SetEasing(Ease.Type.BackOut);
                }
            }

            if (rewardCoinIcon != null)
            {
                rewardCoinIcon.gameObject.SetActive(true);

                if (rewardDisplayPanel == null)
                {
                    rewardCoinIcon.transform.localScale = Vector3.zero;
                    rewardCoinIcon.transform.DOScale(1f, 0.3f).SetEasing(Ease.Type.BackOut);
                }
            }

            if (claim_btn != null)
            {
                claim_btn.gameObject.SetActive(true);
                claim_btn.transform.localScale = Vector3.zero;
                claim_btn.transform.DOScale(1f, 0.3f).SetEasing(Ease.Type.BackOut);
            }
        }

        private void OnClaimClicked()
        {
            if (!hasChosen || chosenReward <= 0) return;

            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif

            if (claim_btn != null)
                claim_btn.interactable = false;

            if (lifeRecievedAudio != null)
                SoundManager.PlaySound(lifeRecievedAudio);

            // Turn off particles immediately when claim is clicked
            TurnOffParticles();

            SetChestsInteractable(false);

            if (chosenChestImage != null && currencyTargetPanel != null)
            {
                RectTransform chestRect = chosenChestImage.rectTransform;

                FloatingCloud.SpawnCurrency(
                    CurrencyType.Coins.ToString(),
                    chestRect,
                    currencyTargetPanel,
                    coinAnimationCount,
                    "",
                    () =>
                    {
                        EconomyManager.Add(CurrencyType.Coins, chosenReward);

                        PlayerPrefs.SetString(PREF_LAST_BONUS_TICKS, DateTime.Now.Ticks.ToString());
                        PlayerPrefs.Save();

                        isRewardAvailable = false;
                        nextAvailableTime = DateTime.Now.AddHours(COOLDOWN_HOURS);

                        ResetUIAfterClaim();

                        Debug.Log($"[DailyBonus] Claimed {chosenReward} coins!");

                        StartCoroutine(CloseAfterDelay(0.5f));
                    }
                );
            }
            else
            {
                EconomyManager.Add(CurrencyType.Coins, chosenReward);

                PlayerPrefs.SetString(PREF_LAST_BONUS_TICKS, DateTime.Now.Ticks.ToString());
                PlayerPrefs.Save();

                isRewardAvailable = false;
                nextAvailableTime = DateTime.Now.AddHours(COOLDOWN_HOURS);

                ResetUIAfterClaim();

                Debug.Log($"[DailyBonus] Claimed {chosenReward} coins! (No animation - missing references)");

                StartCoroutine(CloseAfterDelay(1.5f));
            }
        }

        private IEnumerator CloseAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            ScreenManager.CloseScreen<CrystalUIDailyBonus>();
        }

        private void ResetUIAfterClaim()
        {
            SetChestsInteractable(false);

            hasChosen = false;
            chosenReward = 0;
            chosenChestImage = null;

            Vector3 chestTargetScale = Vector3.one * chestScale;

            if (safeChestImage != null && closedChestSprite != null)
            {
                safeChestImage.sprite = closedChestSprite;
                safeChestImage.SetNativeSize();
                safeChestImage.transform.localScale = chestTargetScale;
            }

            if (riskChestImage != null && closedChestSprite != null)
            {
                riskChestImage.sprite = closedChestSprite;
                riskChestImage.SetNativeSize();
                riskChestImage.transform.localScale = chestTargetScale;
            }

            HideRewardDisplay();

            // Ensure particles are off after claim
            TurnOffParticles();

            if (claim_btn != null)
            {
                claim_btn.gameObject.SetActive(false);
                claim_btn.interactable = true;
            }

            UpdateBottomInstructionText();
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
            if (chest != null)
                chest.localScale = Vector3.zero;

            if (claimButton != null)
                claimButton.localScale = Vector3.zero;

            if (adButton != null)
                adButton.localScale = Vector3.zero;

            if (backgroundImage != null)
            {
                var color = backgroundImage.color;
                color.a = 0f;
                backgroundImage.color = color;
                backgroundImage.DOFade(1f, 0.3f);
            }

            if (safeChestImage != null)
            {
                safeChestImage.transform.localScale = Vector3.zero;
                Vector3 targetScale = Vector3.one * chestScale;
                safeChestImage.transform.DOScale(targetScale * 1.1f, 0.4f).SetEasing(Ease.Type.BackOut)
                    .OnComplete(() => safeChestImage.transform.DOScale(targetScale, 0.1f));
            }

            if (riskChestImage != null)
            {
                riskChestImage.transform.localScale = Vector3.zero;
                Vector3 targetScale = Vector3.one * chestScale;
                riskChestImage.transform.DOScale(targetScale * 1.1f, 0.4f, 0.1f).SetEasing(Ease.Type.BackOut)
                    .OnComplete(() => riskChestImage.transform.DOScale(targetScale, 0.1f));
            }

            if (claimButton != null)
            {
                claimButton.DOScale(1.05f, 0.3f, 0.3f).SetEasing(Ease.Type.BackOut)
                    .OnComplete(() => claimButton.DOScale(1f, 0.1f));
            }

            if (adButton != null)
            {
                adButton.DOScale(1.05f, 0.3f, 0.4f).SetEasing(Ease.Type.BackOut)
                    .OnComplete(() => adButton.DOScale(1f, 0.1f));
            }

            ScreenManager.OnPageOpened(this);
            ScreenManager.OnPopupWindowOpened(this);
        }

        public override void PlayHideAnimation()
        {
            if (safeChestImage != null)
                safeChestImage.transform.DOScale(0, 0.3f).SetEasing(Ease.Type.BackIn);

            if (riskChestImage != null)
                riskChestImage.transform.DOScale(0, 0.3f).SetEasing(Ease.Type.BackIn);

            if (claimButton != null)
                claimButton.DOScale(0, 0.3f).SetEasing(Ease.Type.BackIn);

            if (adButton != null)
                adButton.DOScale(0, 0.3f).SetEasing(Ease.Type.BackIn);

            if (backgroundImage != null)
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
