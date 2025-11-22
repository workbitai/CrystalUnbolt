using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Collections;
using DG.Tweening;

namespace CrystalUnbolt
{
    public class CrystalUIComplete : BaseScreen
    {
        [BoxGroup("Safe Area", "Safe Area")]
        [SerializeField] RectTransform safeAreaTransform;

        [BoxGroup("Fade", "Fade")]
        [SerializeField] UIFadeAnimation backgroundFade;

        [BoxGroup("Level Complete", "Level Complete")]
        [SerializeField] UIScaleAnimation levelCompleteLabel;
        [BoxGroup("Level Complete")]
        [SerializeField] RectTransform congratsRibbon;
        [BoxGroup("Level Complete")]
        [SerializeField] RectTransform[] stars; // Array of 3 stars

        [BoxGroup("CrystalReward", "CrystalReward")]
        [SerializeField] UIScaleAnimation rewardLabel;
        [BoxGroup("CrystalReward", "CrystalReward")]
        [SerializeField] TextMeshProUGUI rewardAmountText;
        [BoxGroup("CrystalReward", "CrystalReward")]
        [SerializeField] CrystalPURewardPanel powerUpsRewardPanel;

        [BoxGroup("Coins", "Coins")]
        [SerializeField] UIScaleAnimation coinsPanelScalable;
        [BoxGroup("Coins", "Coins")]
        [SerializeField] CrystalCurrencyUIPanelSimple coinsPanelUI;

        [BoxGroup("Buttons", "Buttons")]
        [SerializeField] Button multiplyRewardButton;
        [BoxGroup("Buttons", "Buttons")]
        [SerializeField] Button homeButton;
        [BoxGroup("Buttons", "Buttons")]
        [SerializeField] Button nextLevelButton;

        [BoxGroup("Button Animations", "Button Animations")]
        [SerializeField] UIFadeAnimation multiplyRewardButtonFade;
        [BoxGroup("Button Animations", "Button Animations")]
        [SerializeField] UIScaleAnimation homeButtonScaleAnimation;
        [BoxGroup("Button Animations", "Button Animations")]
        [SerializeField] UIScaleAnimation nextLevelButtonScaleAnimation;

        [SerializeField] ParticleSystem starParticles;
        [SerializeField] ParticleSystem fireParticles;
        // [SerializeField] GameObject starImage; // Removed - using star array animation instead
        private AnimCase noThanksAppearTween;
        private DG.Tweening.Tween levelCompleteLabelLoop; // Store reference to stop continuous animation

        private int coinsHash = "Coins".GetHashCode();
        private int currentReward;

        public override void Init()
        {
            multiplyRewardButton.onClick.AddListener(MultiplyRewardButton);
            homeButton.onClick.AddListener(HomeButton);
            nextLevelButton.onClick.AddListener(NextLevelButton);

            coinsPanelUI.Init();
            powerUpsRewardPanel.Init();

            SafeAreaHandler.RegisterRectTransform(safeAreaTransform);

            // Initialize star positions and scales
            ResetAnimatedElements();
        }

        private void ResetAnimatedElements()
        {
            // Reset stars to hidden state
            if (stars != null)
            {
                foreach (var star in stars)
                {
                    if (star != null)
                    {
                        star.localScale = Vector3.zero;
                        star.gameObject.SetActive(false);
                    }
                }
            }

            // Reset congrats ribbon to off-screen
            if (congratsRibbon != null)
            {
                congratsRibbon.localScale = Vector3.one;
                congratsRibbon.gameObject.SetActive(true); // Ensure it's active

                // Get canvas for proper positioning
                Canvas canvas = congratsRibbon.GetComponentInParent<Canvas>();
                RectTransform canvasRect = canvas != null ? canvas.GetComponent<RectTransform>() : null;

                Vector3 originalPos = congratsRibbon.anchoredPosition;
                float offScreenOffset = 2000f;
                if (canvasRect != null)
                {
                    offScreenOffset = canvasRect.rect.width * 1.5f;
                }

                congratsRibbon.anchoredPosition = new Vector3(originalPos.x - offScreenOffset, originalPos.y, 0);
            }

            // Reset level complete label
            if (levelCompleteLabel != null)
            {
                levelCompleteLabel.Hide(immediately: true);
            }

            // Reset reward label
            if (rewardLabel != null)
            {
                rewardLabel.Hide(immediately: true);
            }
        }

        private void OnDestroy()
        {
            powerUpsRewardPanel.Unload();
        }
        public override void PlayShowAnimationMainReturn()
        {

        }
        #region Show/Hide
        public override void PlayShowAnimation()
        {
            // Initialize reward amount
            currentReward = CrystalLevelController.GetCurrentLevelReward();

            // Hide all interactive elements initially
            rewardLabel.Hide(immediately: true);
            multiplyRewardButtonFade.Hide(immediately: true);
            multiplyRewardButton.interactable = false;
            nextLevelButtonScaleAnimation.Hide(immediately: true);
            nextLevelButton.interactable = false;
            homeButtonScaleAnimation.Hide(immediately: true);
            homeButton.interactable = false;
            coinsPanelScalable.Hide(immediately: true);
            levelCompleteLabel.Hide(immediately: true);

            // Play particle effects
            if (starParticles != null) starParticles.Play();
            //   if (fireParticles != null) fireParticles.Play();

            // Start the complete animation sequence
            StartCoroutine(CompleteWinScreenAnimation());
        }

        private IEnumerator CompleteWinScreenAnimation()
        {
            // Step 1: Background fade in
            backgroundFade.Show(duration: 0.4f);
            yield return new WaitForSeconds(0.2f);

            // Step 2: Congrats ribbon slides in from left
            AnimateCongratsRibbon();
            yield return new WaitForSeconds(0.12f);


            // Step 4: "Well Done!" / Level Complete label appears
            // yield return new WaitForSeconds(0.05f);

            AnimateLevelCompleteLabel();
            yield return new WaitForSeconds(0.4f);
            fireParticles.gameObject.SetActive(true);
            if (fireParticles != null) fireParticles.Play();

            // Step 5: Coins panel slides down from top
            AnimateCoinsPanel();
            yield return new WaitForSeconds(0.3f);

            // Step 6: Power-ups reward panel appears
            powerUpsRewardPanel.Show(CrystalLevelController.GetPUReward(), 0.4f);
            yield return new WaitForSeconds(0.4f);

            // Step 7: Reward label with coin counting animation
            if (currentReward > 0)
            {
                AnimateRewardLabel();
                yield return new WaitForSeconds(0.5f);

                // Step 8: Multiply reward button fades in
                AnimateMultiplyButton();
                yield return new WaitForSeconds(0.3f);

                // Step 9: Spawn currency and animate bottom buttons
                SpawnCurrencyAndShowButtons();
            }
            else
            {
                // No reward, just show buttons
                AnimateBottomButtons();
            }
        }

        private void AnimateCongratsRibbon()
        {
            if (congratsRibbon == null) return;

            // Make sure ribbon is active
            congratsRibbon.gameObject.SetActive(true);

            // Get the canvas to calculate proper positioning
            Canvas canvas = congratsRibbon.GetComponentInParent<Canvas>();
            RectTransform canvasRect = canvas != null ? canvas.GetComponent<RectTransform>() : null;

            // Store the target position (where it should end up on screen)
            // Position it in the center of the screen horizontally
            Vector3 targetPos;
            if (canvasRect != null)
            {
                // Center horizontally (x = 0 is center in canvas space)
                float targetX = 0f; // Center of screen
                targetPos = new Vector3(targetX, congratsRibbon.anchoredPosition.y, 0);
            }
            else
            {
                // Fallback: use center position
                targetPos = new Vector3(0f, congratsRibbon.anchoredPosition.y, 0);
            }

            // Calculate off-screen starting position
            float offScreenOffset = 2000f;
            if (canvasRect != null)
            {
                offScreenOffset = canvasRect.rect.width * 1.2f; // Well off-screen to the left
            }

            // Start from left side, well off-screen
            Vector3 startPos = new Vector3(targetPos.x - offScreenOffset, targetPos.y, 0);
            congratsRibbon.anchoredPosition = startPos;

            // Slide in with bounce effect
            Sequence ribbonSeq = DOTween.Sequence();
            ribbonSeq.Append(congratsRibbon.DOAnchorPosX(targetPos.x + 50f, 0.6f).SetEase(DG.Tweening.Ease.OutBack));
            ribbonSeq.Append(congratsRibbon.DOAnchorPosX(targetPos.x, 0.3f).SetEase(DG.Tweening.Ease.InOutSine));
            ribbonSeq.Join(congratsRibbon.DOPunchScale(Vector3.one * 0.15f, 0.4f, 4, 0.5f));
        }

        private void AnimateStarsSequence()
        {
            if (stars == null || stars.Length == 0) return;

            for (int i = 0; i < stars.Length; i++)
            {
                if (stars[i] != null)
                {
                    stars[i].gameObject.SetActive(true);
                    float finalScale = (i == 1) ? 0.32f : 0.23f; // Middle star is larger
                    float delay = i * 0.15f;

                    AnimateStarWithBounce(stars[i], delay, finalScale);
                }
            }
        }

        private void AnimateStarWithBounce(RectTransform star, float delay, float finalScale)
        {
            Vector3 originalPos = star.anchoredPosition;
            star.localScale = Vector3.zero;
            star.anchoredPosition = new Vector3(originalPos.x, originalPos.y + 300f, 0);

            Tween.DelayedCall(delay, () =>
            {
                Sequence starSeq = DOTween.Sequence();

                // Drop down with bounce
                starSeq.Append(star.DOAnchorPosY(originalPos.y - 30f, 0.4f).SetEase(DG.Tweening.Ease.OutBounce));
                starSeq.Append(star.DOAnchorPosY(originalPos.y, 0.2f).SetEase(DG.Tweening.Ease.OutSine));

                // Scale up with elastic effect (run separately since DOScale returns AnimCase)
                star.DOScale(Vector3.one * finalScale * 1.3f, 0.4f).SetEasing(Ease.Type.ElasticOut).OnComplete(() =>
                {
                    star.DOScale(Vector3.one * finalScale, 0.2f).SetEasing(Ease.Type.SineInOut).OnComplete(() =>
                    {
                        // Final celebration punch
                        star.DOPunchScale(Vector3.one * finalScale * 0.25f, 0.4f, 5, 0.5f);
                    });
                });
            });
        }

        private void AnimateLevelCompleteLabel()
        {
            if (levelCompleteLabel == null) return;

            // Stop any existing loop animation
            if (levelCompleteLabelLoop != null && levelCompleteLabelLoop.IsActive())
            {
                levelCompleteLabelLoop.Kill();
                levelCompleteLabelLoop = null;
            }

            levelCompleteLabel.Show(1.2f, 0.3f, 0f, false, () =>
            {
                // Add a subtle glow effect that stops after 3 seconds
                levelCompleteLabelLoop = levelCompleteLabel.Transform.DOPunchScale(Vector3.one * 0.05f, 0.4f, 2, 0.3f)
                    .SetLoops(5, LoopType.Yoyo) // Loop 5 times instead of infinite
                    .OnComplete(() =>
                    {
                        // Ensure it returns to normal scale after animation
                        levelCompleteLabel.Transform.localScale = Vector3.one;
                        levelCompleteLabelLoop = null;
                    });
            });
        }

        private void AnimateCoinsPanel()
        {
            if (coinsPanelScalable == null) return;

            // Slide down from top with scale
            RectTransform coinsRect = coinsPanelScalable.Transform as RectTransform;
            if (coinsRect != null)
            {
                Vector3 originalPos = coinsRect.anchoredPosition;
                coinsRect.anchoredPosition = new Vector3(originalPos.x, originalPos.y + 200f, 0);
                coinsRect.localScale = Vector3.zero;

                // Animate position and scale separately since DOScale returns AnimCase
                coinsRect.DOAnchorPosY(originalPos.y, 0.4f).SetEase(DG.Tweening.Ease.OutBack);
                coinsRect.DOScale(Vector3.one, 0.4f).SetEasing(Ease.Type.BackOut);
            }
            else
            {
                coinsPanelScalable.Show();
            }
        }

        private void AnimateRewardLabel()
        {
            if (rewardLabel == null) return;

            // Scale in with bounce
            rewardLabel.Show(1.3f, 0.5f, 0f, false, () =>
            {
                // Start counting animation
                ShowRewardLabel(currentReward, false, 0.6f, () =>
                {
                    // Celebration bounce after counting completes
                    rewardLabel.Transform.DOPunchScale(Vector3.one * 0.2f, 0.4f, 6, 0.5f);
                });
            });
        }

        private void AnimateMultiplyButton()
        {
            if (multiplyRewardButtonFade == null) return;

            multiplyRewardButtonFade.Show(0.5f, 0f, false, () =>
            {
                multiplyRewardButton.interactable = true;

                // Add subtle pulse animation
                RectTransform btnRect = multiplyRewardButton.transform as RectTransform;
                if (btnRect != null)
                {
                    btnRect.DOPunchScale(Vector3.one * 0.1f, 0.5f, 2, 0.3f).SetLoops(-1, LoopType.Yoyo);
                }
            });
        }

        private void SpawnCurrencyAndShowButtons()
        {
            FloatingCloud.SpawnCurrency(
                coinsHash,
                (RectTransform)rewardLabel.Transform,
                (RectTransform)coinsPanelScalable.Transform,
                10,
                "",
                () =>
                {
                    EconomyManager.Add(CurrencyType.Coins, currentReward);
                    CrystalCloudProgressSync.PushLocalToCloud();
                    AnimateBottomButtons();
                });
        }

        private void AnimateBottomButtons()
        {
            // Animate HOME button
            if (homeButtonScaleAnimation != null)
            {
                homeButtonScaleAnimation.Show(1.15f, 0.4f, 0f, false, () =>
                {
                    homeButton.interactable = true;
                });
            }
            else
            {
                homeButton.interactable = true;
            }

            // Animate NEXT button with slight delay
            if (nextLevelButtonScaleAnimation != null)
            {
                nextLevelButtonScaleAnimation.Show(1.15f, 0.4f, 0.1f, false, () =>
                {
                    nextLevelButton.interactable = true;
                });
            }
            else
            {
                nextLevelButton.interactable = true;
            }
        }

        public override void PlayHideAnimation()
        {
            // Stop any running animations
            if (levelCompleteLabelLoop != null && levelCompleteLabelLoop.IsActive())
            {
                levelCompleteLabelLoop.Kill();
                levelCompleteLabelLoop = null;
            }

            // Reset elements for next time
            ResetAnimatedElements();

            ScreenManager.OnPageClosed(this);
        }


        #endregion

        #region RewardLabel

        public void ShowRewardLabel(float rewardAmounts, bool immediately = false, float duration = 0.3f, Action onComplted = null)
        {
            rewardLabel.Show(immediately: immediately);
            if (immediately)
            {
                rewardAmountText.text = "+" + rewardAmounts;
                onComplted?.Invoke();

                return;
            }

            rewardAmountText.text = "+" + 0;

            Tween.DoFloat(0, rewardAmounts, duration, (float value) =>
            {
                rewardAmountText.text = "+" + (int)value;
            }).OnComplete(delegate
            {

                onComplted?.Invoke();
            });
            StartCoroutine(nameof(PlayWinCoinSound));
        }

        IEnumerator PlayWinCoinSound()
        {
            yield return new WaitForSeconds(1f);
            SoundManager.PlaySound(SoundManager.AudioClips.coinCollect);
        }
        #endregion

        #region Buttons

        public void MultiplyRewardButton()
        {
            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif
            Debug.Log("Click MultiplyRewardButton");
            if (noThanksAppearTween != null && noThanksAppearTween.IsActive)
            {
                noThanksAppearTween.Kill();
            }

            homeButton.interactable = false;
            nextLevelButton.interactable = false;

            MyAdsAdapter.ShowRewardBasedVideo((bool success) =>
            {
                Debug.Log("success" + success);
                if (success)
                {
                    int rewardMult = 3;

                    multiplyRewardButtonFade.Hide(immediately: true);
                    multiplyRewardButton.interactable = false;

                    ShowRewardLabel(currentReward * rewardMult, false, 0.3f, delegate
                    {
                        FloatingCloud.SpawnCurrency(coinsHash, (RectTransform)rewardLabel.Transform, (RectTransform)coinsPanelScalable.Transform, 10, "", () =>
                        {
                            EconomyManager.Add(CurrencyType.Coins, currentReward * rewardMult);
                            // After coins change, try push to cloud if signed in
                            CrystalCloudProgressSync.PushLocalToCloud();

                            homeButton.interactable = true;
                            nextLevelButton.interactable = true;
                        });
                    });
                }
                else
                {
                    Debug.Log("else ");
                    NextLevelButton();
                }
            });
        }

        public void NextLevelButton()
        {
            if (!CrystalGameManager.Data.InfiniteLevels && CrystalLevelController.MaxReachedLevelIndex >= CrystalLevelController.Database.AmountOfLevels)
            {
                CrystalLevelController.ClampMaxReachedLevel();

                HomeButton();
            }
            else
            {
                SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
                Haptic.Play(Haptic.HAPTIC_HARD);
#endif
                ScreenOverlay.Show(0.3f, () =>
                {
                    DataManager.Save(true);

                    ScreenManager.DisableScreen<CrystalUIComplete>();

                    CrystalGameManager.LoadNextLevel();

                    ScreenOverlay.Hide(0.3f);
                });
            }
        }

        public void HomeButton()
        {
            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif
            CrystalLivesSystem.UnlockLife(false);

            ScreenOverlay.Show(0.3f, () =>
            {
                DataManager.Save(true);

                ScreenManager.DisableScreen<CrystalUIComplete>();

                CrystalGameManager.ReturnToMenu();

                ScreenOverlay.Hide(0.3f);
            });
        }

        #endregion
    }
}
