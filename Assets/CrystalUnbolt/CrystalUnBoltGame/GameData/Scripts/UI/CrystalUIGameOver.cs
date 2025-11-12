using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using DG.Tweening;                       
using DGTween = DG.Tweening;             

namespace CrystalUnbolt
{
    public class CrystalUIGameOver : BaseScreen
    {
        [BoxGroup("Safe Area", "Safe Area")]
        [SerializeField] RectTransform safeAreaRectTransform;

        [BoxGroup("Text", "Text")]
        [SerializeField] TMP_Text additionalTimeText;

        [BoxGroup("Buttons", "Buttons")]
        [SerializeField] Button menuButton;
        [BoxGroup("Buttons", "Buttons")]
        [SerializeField] Button replayButton;
        [BoxGroup("Buttons", "Buttons")]
        [SerializeField] Button reviveButton;

        [BoxGroup("Animations", "Animations")]
        [SerializeField] UIScaleAnimation levelFailed;
        [BoxGroup("Animations", "Animations")]
        [SerializeField] UIFadeAnimation backgroundFade;
        [BoxGroup("Animations", "Animations")]
        [SerializeField] UIScaleAnimation menuButtonScalable;
        [BoxGroup("Animations", "Animations")]
        [SerializeField] UIScaleAnimation replayButtonScalable;
        [BoxGroup("Animations", "Animations")]
        [SerializeField] UIScaleAnimation reviveButtonScalable;

        [BoxGroup("Lives", "Lives")]
        [SerializeField] CrystalLivesIndicator livesIndicator;

        private AnimCase continuePingPongCase;

        private Tweener reviveLoop;

        public override void Init()
        {
            menuButton.onClick.AddListener(MenuButton);
            replayButton.onClick.AddListener(ReplayButton);
            reviveButton.onClick.AddListener(ReviveButton);

            SafeAreaHandler.RegisterRectTransform(safeAreaRectTransform);
        }

        public override void PlayShowAnimationMainReturn() { }

        #region Show/Hide

        public override void PlayShowAnimation()
        {
            levelFailed.Hide(immediately: true);
            menuButtonScalable.Hide(immediately: true);
            replayButtonScalable.Hide(immediately: true);
            reviveButtonScalable.Hide(immediately: true);

            float fadeDuration = 0.3f;
            backgroundFade.Show(fadeDuration);

            additionalTimeText.text = $"+{CrystalGameManager.Data.AdditionalTimerTimeOnFail}";
            livesIndicator.Show();

            Tween.DelayedCall(fadeDuration * 0.8f, () =>
            {
                levelFailed.Show();

                menuButtonScalable.Show(scaleMultiplier: 1.05f, delay: 0.75f);
                replayButtonScalable.Show(scaleMultiplier: 1.05f, delay: 0.75f);
                reviveButtonScalable.Show(scaleMultiplier: 1.05f, delay: 0.25f);

                Tween.DelayedCall(0.5f, StartReviveButtonAnimation);

                ScreenManager.OnPageOpened(this);
            });
        }

        public override void PlayHideAnimation()
        {
            backgroundFade.Hide(0.3f);
            livesIndicator.Hide();

            Tween.DelayedCall(0.3f, () =>
            {
                StopReviveButtonAnimation();
                ScreenManager.OnPageClosed(this);
            });
        }

        #endregion

        #region Revive Button Animation (DOTween infinite loop)

        private void StartReviveButtonAnimation()
        {
            StopReviveButtonAnimation(); 

            if (reviveButtonScalable != null && reviveButtonScalable.Transform != null)
                reviveButtonScalable.Transform.localScale = Vector3.one;

            reviveLoop = DG.Tweening.ShortcutExtensions.DOScale(
                  reviveButtonScalable.Transform, 1.10f, 0.25f)   
              .SetEase(DG.Tweening.Ease.InOutSine)
              .SetLoops(-1, DG.Tweening.LoopType.Yoyo)
              .SetUpdate(true);
        }

        private void StopReviveButtonAnimation()
        {
            if (reviveLoop != null && reviveLoop.IsActive())
                reviveLoop.Kill();
            reviveLoop = null;

            if (continuePingPongCase != null && continuePingPongCase.IsActive)
                continuePingPongCase.Kill();
            continuePingPongCase = null;
        }

        #endregion

        #region Buttons

        private void ReviveButton()
        {
            StopReviveButtonAnimation();

            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
           Haptic.Play(Haptic.HAPTIC_HARD);
#endif
            MyAdsAdapter.ShowRewardBasedVideo(ReviveCallback);
        }

        private void ReviveCallback(bool watchedRV)
        {
            if (!watchedRV) return;

            ScreenManager.CloseScreen<CrystalUIGameOver>();
            ScreenManager.DisplayScreen<CrystalUIGame>();

            CrystalLevelController.GameTimer.SetMaxTime(CrystalGameManager.Data.AdditionalTimerTimeOnFail);
            CrystalLevelController.GameTimer.Reset();
            CrystalLevelController.GameTimer.Start();

            CrystalGameManager.Revive();
        }

        private void ReplayButton()
        {
            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
           Haptic.Play(Haptic.HAPTIC_HARD);
#endif
            if (CrystalLivesSystem.Lives > 0 || CrystalLivesSystem.InfiniteMode)
            {
                CrystalLivesSystem.UnlockLife(true);
                ScreenManager.CloseScreen<CrystalUIGameOver>();
                CrystalGameManager.ReplayLevel();
            }
            else
            {
                CrystalUIAddLivesPanel.Show(lifeAdded =>
                {
                    if (lifeAdded)
                    {
                        ScreenManager.CloseScreen<CrystalUIGameOver>();
                        CrystalGameManager.ReplayLevel();
                    }
                });
            }

            DataManager.Save();
        }

        private void MenuButton()
        {
            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
           Haptic.Play(Haptic.HAPTIC_HARD);
#endif
            ScreenManager.CloseScreen<CrystalUIGameOver>(() =>
            {
                CrystalGameManager.ReturnToMenu();
            });
        }

        #endregion
    }
}
