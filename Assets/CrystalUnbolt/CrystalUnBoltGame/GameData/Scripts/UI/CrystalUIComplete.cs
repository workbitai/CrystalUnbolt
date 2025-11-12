using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Collections;

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
        [SerializeField] GameObject starImage;
        private AnimCase noThanksAppearTween;

        private int coinsHash = "Coins".GetHashCode();
        private int currentReward;

        public override void Init()
        {
            if (starImage != null)
                starImage.transform.localScale = Vector3.zero;
            multiplyRewardButton.onClick.AddListener(MultiplyRewardButton);
            homeButton.onClick.AddListener(HomeButton);
            nextLevelButton.onClick.AddListener(NextLevelButton);

            coinsPanelUI.Init();
            powerUpsRewardPanel.Init();

            SafeAreaHandler.RegisterRectTransform(safeAreaTransform);
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


            if (starParticles != null)
            {
                starParticles.Play();
                fireParticles.Play();
                StartCoroutine(ShowImageAfterParticles());
            }




            rewardLabel.Hide(immediately: true);
            multiplyRewardButtonFade.Hide(immediately: true);
            multiplyRewardButton.interactable = false;
            nextLevelButtonScaleAnimation.Hide(immediately: true);
            nextLevelButton.interactable = false;
            homeButtonScaleAnimation.Hide(immediately: true);
            homeButton.interactable = false;
            coinsPanelScalable.Hide(immediately: true);

            currentReward = CrystalLevelController.GetCurrentLevelReward();

            backgroundFade.Show(duration: 0.3f);
            levelCompleteLabel.Show();

            coinsPanelScalable.Show();

            powerUpsRewardPanel.Show(CrystalLevelController.GetPUReward(), 0.5f);

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
                            // After coins change, try push to cloud if signed in
                            CrystalCloudProgressSync.PushLocalToCloud();

                            homeButtonScaleAnimation.Show(1.05f, 0.25f, 1f);
                            nextLevelButtonScaleAnimation.Show(1.05f, 0.25f, 1f);

                            homeButton.interactable = true;
                            nextLevelButton.interactable = true;
                        });
                    });
                });
            }
            else
            {
                rewardLabel.Hide(immediately: true);

                homeButtonScaleAnimation.Show(1.05f, 0.25f, 1f);
                nextLevelButtonScaleAnimation.Show(1.05f, 0.25f, 1f);

                homeButton.interactable = true;
                nextLevelButton.interactable = true;
            }
        }
        private IEnumerator ShowImageAfterParticles()
        {
            // Particle khatam hone tak wait
            float duration = starParticles.main.duration;
            yield return new WaitForSeconds(duration + 0.2f);

            if (starImage != null)
            {
                // Scale 0 ? 1 animation
                float time = 0f;
                while (time < 0.1f)
                {
                    float t = time / 0.1f;
                    starImage.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
                    time += Time.deltaTime;
                    yield return null;
                }
                starImage.transform.localScale = Vector3.one; // final size
            }
        }
        public override void PlayHideAnimation()
        {
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
