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
                        star.localScale = Vector3.zero;
                }
            }
            
            // Reset congrats ribbon to off-screen
            if (congratsRibbon != null)
            {
                congratsRibbon.localScale = Vector3.one;
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
            // Play particles
            if (starParticles != null)
            {
                starParticles.Play();
                fireParticles.Play();
            }

            // Hide all elements initially
            rewardLabel.Hide(immediately: true);
            multiplyRewardButtonFade.Hide(immediately: true);
            multiplyRewardButton.interactable = false;
            nextLevelButtonScaleAnimation.Hide(immediately: true);
            nextLevelButton.interactable = false;
            homeButtonScaleAnimation.Hide(immediately: true);
            homeButton.interactable = false;
            coinsPanelScalable.Hide(immediately: true);

            currentReward = CrystalLevelController.GetCurrentLevelReward();

            // Start animation sequence
            backgroundFade.Show(duration: 0.3f);
            
            // NEW ANIMATIONS - Exciting celebration sequence!
            StartCoroutine(CelebrationAnimationSequence());
        }

        private IEnumerator CelebrationAnimationSequence()
        {
            // Step 1: Animate stars one by one (0.5s)
            AnimateStars();
            yield return new WaitForSeconds(0.5f);
            
            // Step 2: Animate CONGRATS ribbon (0.3s)
            AnimateCongratsRibbon();
            yield return new WaitForSeconds(0.3f);
            
            // Step 3: Animate LEVEL COMPLETED text (0.3s)
            levelCompleteLabel.Show();
            yield return new WaitForSeconds(0.3f);
            
            // Step 4: Show coins panel at top
            coinsPanelScalable.Show();
            yield return new WaitForSeconds(0.2f);
            
            // Step 5: Show power-ups reward panel
            powerUpsRewardPanel.Show(CrystalLevelController.GetPUReward(), 0.5f);
            yield return new WaitForSeconds(0.3f);
            
            // Step 6: Show reward and buttons
            if (currentReward > 0)
            {
                ShowRewardLabel(currentReward, false, 0.3f, delegate
                {
                    rewardLabel.Transform.DOPushScale(Vector3.one * 1.1f, Vector3.one, 0.2f, 0.2f).OnComplete(delegate
                    {
                        multiplyRewardButtonFade.Show();
                        multiplyRewardButton.interactable = true;

                        FloatingCloud.SpawnCurrency(coinsHash, (RectTransform)rewardLabel.Transform, (RectTransform)coinsPanelScalable.Transform, 10, "", () =>
                        {
                            EconomyManager.Add(CurrencyType.Coins, currentReward);
                            CrystalCloudProgressSync.PushLocalToCloud();

                            AnimateBottomButtons();
                        });
                    });
                });
            }
            else
            {
                rewardLabel.Hide(immediately: true);
                AnimateBottomButtons();
            }
        }
        
        private void AnimateStars()
        {
            if (stars == null || stars.Length == 0) return;
            
            for (int i = 0; i < stars.Length; i++)
            {
                if (stars[i] != null)
                {
                    // Left (0) and Right (2) stars = 0.23 scale
                    // Middle (1) star = 0.32 scale
                    float finalScale = (i == 1) ? 0.32f : 0.23f;
                    AnimateStar(stars[i], i * 0.15f, finalScale);
                }
            }
        }
        
        private void AnimateStar(RectTransform star, float delay, float finalScale)
        {
            // Start small and from above
            star.localScale = Vector3.zero;
            Vector3 originalPos = star.anchoredPosition;
            star.anchoredPosition = new Vector3(originalPos.x, originalPos.y + 200f, 0);
            
            Tween.DelayedCall(delay, () =>
            {
                // Drop down with bounce
                star.DOAnchorPos(originalPos, 0.5f).SetEase(DG.Tweening.Ease.OutBounce);
                
                // Scale up with elastic bounce to final scale
                star.DOScale(Vector3.one * finalScale, 0.5f).SetEasing(Ease.Type.ElasticOut).OnComplete(() =>
                {
                    // Small punch for extra celebration (proportional to final scale)
                    star.DOPunchScale(Vector3.one * finalScale * 0.2f, 0.3f, 6, 0.5f);
                });
            });
        }
        
        private void AnimateCongratsRibbon()
        {
            if (congratsRibbon == null) return;
            
            // Start from left side of screen
            Vector3 originalPos = congratsRibbon.anchoredPosition;
            congratsRibbon.anchoredPosition = new Vector3(originalPos.x - 800f, originalPos.y, 0);
            congratsRibbon.localScale = Vector3.one;
            
            // Slide in from left with overshoot
            congratsRibbon.DOAnchorPos(originalPos, 0.6f).SetEase(DG.Tweening.Ease.OutBack);
        }
        
        private void AnimateBottomButtons()
        {
            // Animate HOME and NEXT buttons with bounce
            homeButtonScaleAnimation.Show(1.05f, 0.25f, 1f);
            nextLevelButtonScaleAnimation.Show(1.05f, 0.25f, 1f);

            homeButton.interactable = true;
            nextLevelButton.interactable = true;
        }
        
        public override void PlayHideAnimation()
        {
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
