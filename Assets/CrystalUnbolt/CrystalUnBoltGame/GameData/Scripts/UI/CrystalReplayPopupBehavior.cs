using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalUnbolt
{
    public class CrystalReplayPopupBehavior : MonoBehaviour, IPopupWindow
    {
        [SerializeField] Canvas canvas;
        [SerializeField] Image fadeImage;
        [SerializeField] RectTransform retryImage;
        [SerializeField] RectTransform panel;
        [SerializeField] Button adButton;
        [SerializeField] Button coinsButton;
        [SerializeField] TMP_Text coinsText;
        [SerializeField] Button exitButton;

        public bool IsOpened => canvas.enabled;

        public void Awake()
        {
            adButton.onClick.AddListener(OnAdsButtonClicked);
            coinsButton.onClick.AddListener(OnCoinsButtonClicked);
            exitButton.onClick.AddListener(Hide);
        }

        public void Show()
        {
           // if (CrystalLevelController.isRealGameFinish) return;
            canvas.enabled = true;
            fadeImage.SetAlpha(0);
            fadeImage.DOFade(0.3f, 0.3f);
            panel.anchoredPosition = Vector2.down * 2000;
            panel.DOAnchoredPosition(Vector2.zero, 0.3f).SetEasing(Ease.Type.SineOut);

            coinsText.text = CrystalGameManager.Data.ReplayStageCost.ToString();

            if (CrystalGameManager.Data.GameplayTimerEnabled) CrystalLevelController.GameTimer.Pause();

            retryImage.DORotate(
     new Vector3(0, 0, 360),   
     0.5f,                     
     RotateMode.FastBeyond360  
 );


            ScreenManager.OnPopupWindowOpened(this);
        }

        public void Hide()
        {
#if MODULE_HAPTIC
           Haptic.Play(Haptic.HAPTIC_HARD);
#endif
            fadeImage.DOFade(0, 0.3f);
            panel.DOAnchoredPosition(Vector2.down * 2000, 0.3f).SetEasing(Ease.Type.SineIn).OnComplete(() =>
            {
                canvas.enabled = false;

                if (CrystalGameManager.Data.GameplayTimerEnabled) CrystalLevelController.GameTimer.Resume();
            });

            ScreenManager.OnPopupWindowClosed(this);
        }

        private void OnAdsButtonClicked()
        {
           // if (CrystalLevelController.isRealGameFinish) return;
            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
           Haptic.Play(Haptic.HAPTIC_HARD);
#endif
            MyAdsAdapter.ShowRewardBasedVideo((bool success) =>
            {
                if (success)
                {
                    Hide();
                    CrystalGameManager.ReplayStage();
                }
            });
        }

        private void OnCoinsButtonClicked()
        {
          //  if (CrystalLevelController.isRealGameFinish) return;
            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
           Haptic.Play(Haptic.HAPTIC_HARD);
#endif
            if (EconomyManager.Get(CurrencyType.Coins) >= CrystalGameManager.Data.ReplayStageCost)
            {
                EconomyManager.Substract(CurrencyType.Coins, CrystalGameManager.Data.ReplayStageCost);

                Hide();

                CrystalGameManager.ReplayStage();
            }
        }
    }
}
