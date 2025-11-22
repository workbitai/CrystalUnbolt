using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalUnbolt
{
    public class CrystalTimerStartPopup : MonoBehaviour, IPopupWindow
    {
        [SerializeField] Canvas canvas;
        [SerializeField] Image fadeImage;
        [SerializeField] RectTransform panel;
        [SerializeField] TMP_Text messageText;
        [SerializeField] Button continueButton;

        public bool IsOpened => canvas.enabled;

        private GameCallback onContinueCallback;

        private void Awake()
        {
            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinueButtonClicked);
            
            // Ensure popup starts hidden
            if (canvas != null)
            {
                canvas.enabled = false;
            }
            if (gameObject != null)
            {
                gameObject.SetActive(true); // Keep GameObject active but canvas disabled
            }
        }
        
        private void OnEnable()
        {
            // If popup is enabled but we're not on Level 11, hide it immediately
            if (canvas != null && !canvas.enabled)
            {
                if (fadeImage != null) fadeImage.SetAlpha(0);
                if (panel != null) panel.anchoredPosition = Vector2.down * 2000;
            }
        }

        public void Show(GameCallback onContinue = null)
        {
            // Note: Level check is done in CrystalUIGame before calling Show()
            int currentLevel = CrystalLevelController.DisplayedLevelIndex + 1;
            int displayedIndex = CrystalLevelController.DisplayedLevelIndex;
            
            Debug.Log($"[TimerStartPopup] Show() called - Level: {currentLevel}, Index: {displayedIndex}");
            
            onContinueCallback = onContinue;
            
            // Ensure popup is active and visible
            if (canvas != null)
            {
                canvas.enabled = true;
                gameObject.SetActive(true);
            }
            
            if (fadeImage != null)
            {
                fadeImage.SetAlpha(0);
                fadeImage.DOFade(0.3f, 0.3f);
            }
            
            if (panel != null)
            {
                panel.anchoredPosition = Vector2.down * 2000;
                panel.DOAnchoredPosition(Vector2.zero, 0.3f).SetEasing(Ease.Type.SineOut);
            }

            if (messageText != null)
            {
                messageText.text = "Timer is starting from this level!";
            }

            ScreenManager.OnPopupWindowOpened(this);
            Debug.Log("[TimerStartPopup] Popup shown successfully");
        }

        public void Hide(bool immediately = false)
        {
#if MODULE_HAPTIC
            if (!immediately)
                Haptic.Play(Haptic.HAPTIC_HARD);
#endif
            if (immediately)
            {
                if (canvas != null) canvas.enabled = false;
                if (fadeImage != null) fadeImage.SetAlpha(0);
                if (panel != null) panel.anchoredPosition = Vector2.down * 2000;
                ScreenManager.OnPopupWindowClosed(this);
                return;
            }
            
            fadeImage.DOFade(0, 0.3f);
            panel.DOAnchoredPosition(Vector2.down * 2000, 0.3f).SetEasing(Ease.Type.SineIn).OnComplete(() =>
            {
                canvas.enabled = false;
                ScreenManager.OnPopupWindowClosed(this);
            });
        }

        private void OnContinueButtonClicked()
        {
            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif
            onContinueCallback?.Invoke();
            Hide();
        }
    }
}


