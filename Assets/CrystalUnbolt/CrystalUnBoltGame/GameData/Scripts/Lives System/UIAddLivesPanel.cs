using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // DOTween

namespace CrystalUnbolt
{
    public class CrystalUIAddLivesPanel : BaseScreen, IPopupWindow
    {
        [Header("Panel")]
        [SerializeField] private RectTransform panel;
        [SerializeField] private Vector2 hidePos; // anchoredPosition uses Vector2

        [Header("Visuals")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private RectTransform heartIcon;
        [SerializeField] private RectTransform adbutton; // pulse target

        [Header("Buttons")]
        [SerializeField] private Button button;
        [SerializeField] private Button closeButton;

        [Header("Lives UI")]
        [SerializeField] private GameObject timerGameObject;
        [SerializeField] private GameObject fullGameObject;
        [SerializeField] private TMP_Text livesAmountText;
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private AudioClip lifeRecievedAudio;

        private Vector2 showPos;   // anchoredPosition at show
        private Color backColor;

        public bool IsOpened => canvas.enabled;

        private SimpleBoolCallback panelClosed;

        // --- DOTween pulse state ---
        private DG.Tweening.Tween adButtonLoop;
        private Vector3 adButtonInitScale;

        private void OnEnable()
        {
            CrystalLivesSystem.StatusChanged += OnStatusChanged;
        }

        private void OnDisable()
        {
            CrystalLivesSystem.StatusChanged -= OnStatusChanged;
            StopAdButtonPulse(); // ensure kill when disabled
        }

        public override void Init()
        {
            backColor = backgroundImage != null ? backgroundImage.color : Color.black;
            showPos = panel != null ? panel.anchoredPosition : Vector2.zero;

            if (button != null) button.onClick.AddListener(OnButtonClick);
            if (closeButton != null) closeButton.onClick.AddListener(OnCloseButtonClicked);

            OnStatusChanged(CrystalLivesSystem.Status);

            if (adbutton != null) adButtonInitScale = adbutton.localScale;

            panelClosed = null;
        }

        public override void PlayShowAnimationMainReturn() { }

        public override void PlayShowAnimation()
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = Color.clear;
                backgroundImage.DOColor(backColor, 0.3f);
            }

            if (panel != null)
            {
                panel.anchoredPosition = hidePos;
                // Use DOTween UI method so SetEase works with DG.Tweening.Ease
                panel.DOAnchorPos(showPos, 0.3f)
                     .SetEase(DG.Tweening.Ease.OutSine);
            }

            ScreenManager.OnPageOpened(this);
            ScreenManager.OnPopupWindowOpened(this);

            if (heartIcon != null) PopupHelper.ShowPopup(heartIcon);
            if (button != null) PopupHelper.ShowPopup(button.transform);

            StartAdButtonPulse(); // start continuous zoom
        }

        public override void PlayHideAnimation()
        {
            StopAdButtonPulse();

            if (backgroundImage != null)
                backgroundImage.DOColor(Color.clear, 0.3f);

            if (panel != null)
            {
                panel.DOAnchorPos(hidePos, 0.3f)
                     .SetEase(DG.Tweening.Ease.InSine)
                     .OnComplete(() =>
                     {
                         ScreenManager.OnPageClosed(this);
                         ScreenManager.OnPopupWindowClosed(this);
                     });
            }
            else
            {
                ScreenManager.OnPageClosed(this);
                ScreenManager.OnPopupWindowClosed(this);
            }
        }

        // ===== Lives status =====
        private void OnStatusChanged(CrystalLivesStatus status)
        {
            if (livesAmountText != null)
                livesAmountText.text = status.LivesCount.ToString();

            if (status.NewLifeTimerEnabled)
            {
                if (timerGameObject) timerGameObject.SetActive(true);
                if (fullGameObject) fullGameObject.SetActive(false);

                if (timeText != null)
                    timeText.text = CrystalLivesSystem.GetFormatedTime(status.NewLifeTime);
            }
            else
            {
                if (timerGameObject) timerGameObject.SetActive(false);
                if (fullGameObject) fullGameObject.SetActive(true);
            }
        }

        public void OnCloseButtonClicked()
        {
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif
            ScreenManager.CloseScreen<CrystalUIAddLivesPanel>();
            panelClosed?.Invoke(false);
        }

        public void OnButtonClick()
        {
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif
            MyAdsAdapter.ShowRewardBasedVideo(success =>
            {
                ScreenManager.CloseScreen<CrystalUIAddLivesPanel>();

                if (success)
                {
                    CrystalLivesSystem.AddLife(1, true);

                    if (lifeRecievedAudio != null)
                        SoundManager.PlaySound(lifeRecievedAudio);

                    panelClosed?.Invoke(true);
                }
            });
        }

        // ===== Static helpers =====
        public static void Show(SimpleBoolCallback onPanelClosed = null)
        {
            CrystalUIAddLivesPanel addLivesPanel = ScreenManager.GetPage<CrystalUIAddLivesPanel>();
            if (addLivesPanel != null)
            {
                addLivesPanel.panelClosed = onPanelClosed;
                ScreenManager.DisplayScreen<CrystalUIAddLivesPanel>();
            }
            else
            {
                onPanelClosed?.Invoke(false);
            }
        }

        public static bool Exists()
        {
            return ScreenManager.GetPage<CrystalUIAddLivesPanel>() != null;
        }

        #region Revive Button Animation (DOTween infinite loop)

        /// <summary>
        /// adbutton par continuous zoom in/out pulse start kare.
        /// </summary>
        public void StartAdButtonPulse(float targetScale = 1.10f, float duration = 0.25f)
        {
            StopAdButtonPulse();
            if (adbutton == null) return;

            if (adButtonInitScale == default)
                adButtonInitScale = adbutton.localScale;

            adbutton.localScale = adButtonInitScale;

            adButtonLoop = DG.Tweening.ShortcutExtensions
                .DOScale(adbutton, adButtonInitScale * targetScale, duration)
                .SetEase(DG.Tweening.Ease.InOutSine)
                .SetLoops(-1, DG.Tweening.LoopType.Yoyo) // infinite
                .SetUpdate(true)                         // run at timescale 0
                .SetLink(adbutton.gameObject);           // auto-kill on destroy
        }

        /// <summary>
        /// pulse stop + scale reset.
        /// </summary>
        public void StopAdButtonPulse()
        {
            if (adButtonLoop != null && DG.Tweening.TweenExtensions.IsActive(adButtonLoop))
                DG.Tweening.TweenExtensions.Kill(adButtonLoop);

            adButtonLoop = null;

            if (adbutton != null)
                adbutton.localScale = (adButtonInitScale == default) ? Vector3.one : adButtonInitScale;
        }

        #endregion
    }
}
