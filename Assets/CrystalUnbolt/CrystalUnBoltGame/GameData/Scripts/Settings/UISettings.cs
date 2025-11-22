using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;
using DGEase = DG.Tweening.Ease;
using DOShortcuts = DG.Tweening.ShortcutExtensions;


namespace CrystalUnbolt
{
    public class CrystalUISettings : BaseScreen, IPopupWindow
    {
        [BoxGroup("References", "References")]
        [SerializeField] Image backgroundImage;
        [BoxGroup("References", "References")]
        [SerializeField] RectTransform panelRectTransform;
        [BoxGroup("References", "References")]
        [SerializeField] RectTransform contentRectTransform;
        public RectTransform ContentRectTransform => contentRectTransform;

        public bool IsOpened => isPageDisplayed;

        [BoxGroup("Buttons", "Buttons")]
        [SerializeField] Button closeButton;
        [SerializeField] Button privacyPolicyButton;

        [SerializeField] private string privacyURL = "https://crystalunbolt.com/privacy";

        [BoxGroup("Success Popup", "Success Popup")]
        [SerializeField] GameObject successPopup;
        [SerializeField] GameObject dataDeletePopup;

        [BoxGroup("Glow Animation", "Glow Animation")]
        [SerializeField] Image glowImage;
        [SerializeField] float glowScaleDuration = 5f;
        [SerializeField] float glowMaxScale = 20f;
        public override void Init()
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
            privacyPolicyButton.onClick.AddListener(ClickPrivacyPolicy);
            backgroundImage.AddEvent(EventTriggerType.PointerDown, OnBackgroundClicked);

            if (glowImage != null)
            {
                glowImage.transform.localScale = Vector3.zero;
                glowImage.gameObject.SetActive(false);
            }
        }
        public override void PlayShowAnimationMainReturn()
        {

        }
        public override void PlayShowAnimation()
        {
            RecalculatePanelSize();

            panelRectTransform.anchoredPosition = Vector2.down * 2000;
            panelRectTransform.DOAnchoredPosition(Vector2.zero, 0.3f).SetEasing(Ease.Type.SineOut);

            ScreenManager.OnPageOpened(this);
            ScreenManager.OnPopupWindowOpened(this);

            PopupHelper.ShowPopup(privacyPolicyButton.transform);

        }
       
        public void ClickPrivacyPolicy()
        {
            Application.OpenURL(privacyURL);
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif
        }
        public override void PlayHideAnimation()
        {
            panelRectTransform.DOAnchoredPosition(Vector2.down * 2000, 0.3f).SetEasing(Ease.Type.SineIn);


            ScreenManager.OnPageClosed(this);
            ScreenManager.OnPopupWindowClosed(this);

        }

        private void RecalculatePanelSize()
        {
            float height = Mathf.Abs(contentRectTransform.sizeDelta.y);

            int childCount = contentRectTransform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform childTransform = contentRectTransform.GetChild(i);
                if (childTransform != null)
                {
                    CrystalSettingsElementsGroup settingsElementsGroup = childTransform.GetComponent<CrystalSettingsElementsGroup>();
                    if (settingsElementsGroup != null)
                    {
                        if (settingsElementsGroup.IsGroupActive())
                        {
                            height += ((RectTransform)childTransform).sizeDelta.y;
                        }
                    }
                    else
                    {
                        if (childTransform.gameObject.activeSelf)
                        {
                            height += ((RectTransform)childTransform).sizeDelta.y;
                        }
                    }
                }
            }

        }

        public void OnCloseButtonClicked()
        {
            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif
            ScreenManager.CloseScreen<CrystalUISettings>();
        }

        private void OnBackgroundClicked(PointerEventData data)
        {
            ScreenManager.CloseScreen<CrystalUISettings>();
        }

        public void ShowSuccessPopup()
        {
            if (successPopup == null)
            {
                Debug.LogError("[Success] Success popup not assigned in Inspector!");
                return;
            }

            panelRectTransform.gameObject.SetActive(false);
            successPopup.SetActive(true);


            StartCoroutine(AutoCloseSuccessPopup());

            Debug.Log("[Success] Showing success popup");
        }
        public void DeletePopUpOpen()
        {
            if (dataDeletePopup == null)
            {
                Debug.LogError("[Success] Success popup not assigned in Inspector!");
                return;
            }

            panelRectTransform.gameObject.SetActive(false);
            dataDeletePopup.SetActive(true);

            Debug.Log("[Success] Showing success popup");
        }
        public void DeleteDataPopUpClose()
        {
            if (dataDeletePopup == null)
            {
                Debug.LogError("[Success] Success popup not assigned in Inspector!");
                return;
            }

            panelRectTransform.gameObject.SetActive(true);
            dataDeletePopup.SetActive(false);

            Debug.Log("[Success] Showing success popup");
        }

        IEnumerator AutoCloseSuccessPopup()
        {
            yield return new WaitForSeconds(2.3f);

            successPopup.SetActive(false);

            panelRectTransform.gameObject.SetActive(true);

            OnCloseButtonClicked();

            Debug.Log("[Success] Auto-closed success popup");
        }

        public void OnGoogleLoginSuccess()
        {
            PlayGlowAnimation();
        }

        public void OnAppleLoginSuccess()
        {
            PlayGlowAnimation();
        }

        private void PlayGlowAnimation()
        {
            if (glowImage == null)
            {
                Debug.LogError("[CrystalUISettings] Glow image not assigned!");
                ShowSuccessPopup();
                return;
            }

            glowImage.gameObject.SetActive(true);
            glowImage.transform.localScale = Vector3.zero;

            StartCoroutine(ContinuousVibration());

            float totalDuration = 1f;           
            float growDuration = 4f;          
            float shrinkDuration = 1f;        
            float shakePower = 10f;

            Sequence glowSequence = DOTween.Sequence();

            glowSequence.Append(
                DOShortcuts.DOScale(glowImage.transform, glowMaxScale, growDuration)
                           .SetEase(DGEase.OutQuad)                    
                           .OnUpdate(() =>
                           {
                               glowImage.rectTransform.anchoredPosition =
                                   UnityEngine.Random.insideUnitCircle * (shakePower * 0.05f);
                           })
            );

            glowSequence.Append(
                DOShortcuts.DOScale(glowImage.transform, Vector3.zero, shrinkDuration)
                           .SetEase(DGEase.InCubic)                     
                           .OnStart(() =>
                           {
                               glowImage.rectTransform.DOShakeAnchorPos(
                                   shrinkDuration, shakePower, 20, 90, false, true);
                           })
            );

            glowSequence.OnComplete(() =>
            {
                glowImage.rectTransform.anchoredPosition = Vector2.zero;
                glowImage.gameObject.SetActive(false);
                ShowSuccessPopup();
            });
        }

        private IEnumerator ContinuousVibration()
        {
            float totalDuration = 3f;
            float vibrationInterval = 0.1f;
            while (totalDuration > 0)
            {
#if MODULE_HAPTIC
                Haptic.Play(Haptic.HAPTIC_MEDIUM);
#endif
                yield return new WaitForSeconds(vibrationInterval);
                totalDuration -= vibrationInterval;
            }
        }

    }
}
