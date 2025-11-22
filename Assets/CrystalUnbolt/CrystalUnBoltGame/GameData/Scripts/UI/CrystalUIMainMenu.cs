using Coffee.UIExtensions;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
// using CrystalUnbolt.IAPStore; // IAP Removed!
using CrystalUnbolt.Map;
using CrystalUnbolt.SkinStore;

namespace CrystalUnbolt
{
    public class CrystalUIMainMenu : BaseScreen
    {
        public readonly float STORE_AD_RIGHT_OFFSET_X = 300F;
        private const bool SKIN_STORE_ENABLED = false;

        [BoxGroup("Safe Area", "Safe Area")]
        [SerializeField] RectTransform safeAreaRectTransform;

        [BoxGroup("Play", "Play")]
        [SerializeField] Button playButton;
        [BoxGroup("Play")]
        [SerializeField] RectTransform tapToPlayRect;
        [BoxGroup("Play")]
        [SerializeField] TMP_Text playButtonText;
        [BoxGroup("PlayButton", "PlayButton")]
        [SerializeField] Button gamePlayButton;
        [BoxGroup("Play")]
        [SerializeField] RectTransform gamePlayRect;

        [BoxGroup("GAMEbg", "Logo")]
        [SerializeField] GameObject gameBG;

        [BoxGroup("Logo", "Logo")]
        [SerializeField] RectTransform gameLogoRect;

        [BoxGroup("Logo", "Logo")]
        [SerializeField] RectTransform spinImageRect;

        [BoxGroup("Level Grid", "Level Grid")]
        [SerializeField] CrystalUILevelGrid levelGrid;

        [BoxGroup("Coins", "Coins")]
        [SerializeField] CrystalCurrencyUIPanelSimple coinsPanel;

        [BoxGroup("Top Bar", "Top Bar")]
        [SerializeField] RectTransform livesPanel;
        [BoxGroup("Top Bar")]
        [SerializeField] RectTransform profilePanel;

        [BoxGroup("Buttons", "Buttons")]
        [SerializeField] CrystalUIMainMenuButton iapStoreButton;
        [BoxGroup("Buttons")]
        [SerializeField] CrystalUIMainMenuButton dailyBonusButton;
        [BoxGroup("Buttons")]
        [SerializeField] CrystalUIMainMenuButton settingButton;
        [BoxGroup("Buttons")]
        [SerializeField] CrystalUIMainMenuButton noAdsButton;
        [BoxGroup("Buttons")]
        [SerializeField] CrystalUIMainMenuButton dailyGift_Plinko;
        [BoxGroup("Buttons")]
        [SerializeField] CrystalUIMainMenuButton leaderBoardButton;

        [BoxGroup("Leaderboard Lock", "Leaderboard Lock")]
        [SerializeField] private Sprite leaderboardLockedSprite;     // Lock sprite when not logged in
        [SerializeField] private Sprite leaderboardUnlockedSprite;   // Normal sprite when logged in
        [SerializeField] private GameObject pleaseLoginText;    // "Please Login First" text
        [SerializeField] private CrystalLoginAuthManager authManager;       // Auth manager reference

        [BoxGroup("Popup", "Popup")]
        [SerializeField] CrystalUINoAdsPopUp noAdsPopUp;

        [BoxGroup("Privacy Popup", "Privacy Popup")]
        [SerializeField] GameObject privacyPopupRoot;
        [SerializeField] GameObject privacyBG;
        [BoxGroup("Privacy Popup")]
        [SerializeField] Button privacyYesButton;
        [BoxGroup("Privacy Popup")]
        [SerializeField] Button privacyNoButton;
        [BoxGroup("Privacy Popup")]
        [SerializeField] Button privacyPolicyButton;
        [BoxGroup("Privacy Popup")]
        [SerializeField] string privacyPolicyUrl = "https://crystalstudio.com/privacy_policy.html";

        private const string PRIVACY_ACCEPTED_KEY = "privacy_accepted_v1";

        // private UIScaleAnimation coinsLabelScalable; // Animation removed

        private AnimCase tapToPlayPingPong;
        private AnimCase gameLogoPingPong;
        private AnimCase showHideStoreAdButtonDelayTweenCase;
        private readonly Dictionary<RectTransform, DG.Tweening.Tweener> bottomButtonIdleTweens = new Dictionary<RectTransform, DG.Tweening.Tweener>();
        private Coroutine playButtonHideRoutine;

        [SerializeField] private ShinyEffectForUGUI[] shinies;

        public Image avatarImage;
        public TextMeshProUGUI nameLabel;
        public Sprite[] avatarSprites;

        private const string GUEST_NAME_KEY = "profile_guest_name";
        private const string GUEST_AVATAR_KEY = "profile_guest_avatar";
        [BoxGroup("Buttons", "Buttons")]
        [SerializeField] Button profileButton, profileBtn_Second;
        private Coroutine _photoLoadRoutine;
        private string _currentPhotoUrl;

        private void OnEnable()
        {
            // IAPManager.PurchaseCompleted += OnAdPurchased; // IAP Removed!

            // Hide grid on enable (will be shown when play button is clicked)
            if (levelGrid != null && levelGrid.canvas != null)
                levelGrid.canvas.enabled = false;

            // Listen for login events
            if (authManager == null)
                authManager = FindObjectOfType<CrystalLoginAuthManager>();

            if (authManager != null)
            {
                authManager.OnSignedIn += OnUserLoggedIn;
                authManager.OnSignedOut += OnUserLoggedOut;
            }
        }

        private void OnDisable()
        {
            // IAPManager.PurchaseCompleted -= OnAdPurchased; // IAP Removed!

            // Unsubscribe from login events
            if (authManager != null)
            {
                authManager.OnSignedIn -= OnUserLoggedIn;
                authManager.OnSignedOut -= OnUserLoggedOut;
            }
        }

        private void OnUserLoggedIn(Firebase.Auth.FirebaseUser user, object additionalData)
        {
            // User logged in - update leaderboard button to unlocked sprite
            UpdateLeaderboardButtonState();
            Debug.Log("[CrystalUIMainMenu] User logged in - Leaderboard button unlocked");
        }

        private void OnUserLoggedOut()
        {
            // User logged out - update leaderboard button to locked sprite
            UpdateLeaderboardButtonState();
            Debug.Log("[CrystalUIMainMenu] User logged out - Leaderboard button locked");
        }

        public override void Init()
        {
            // coinsLabelScalable = new UIScaleAnimation(coinsPanel.transform); // Animation removed
            coinsPanel.Init();

            // iapStoreButton.Init(STORE_AD_RIGHT_OFFSET_X); // IAP Disabled!
            // noAdsButton.Init(STORE_AD_RIGHT_OFFSET_X); // Disabled - keep button at editor position
            // dailyGift_Plinko.Init(STORE_AD_RIGHT_OFFSET_X);
            //  dailyBonusButton.Init(STORE_AD_RIGHT_OFFSET_X);
            // settingButton.Init(STORE_AD_RIGHT_OFFSET_X);
            //  leaderBoardButton.Init(STORE_AD_RIGHT_OFFSET_X);

            dailyBonusButton.Button.onClick.AddListener(DailyBonusButton);
            // iapStoreButton.Button.onClick.AddListener(IAPStoreButton); // IAP Disabled!
            // noAdsButton.Button.onClick.AddListener(NoAdButton); // IAP REMOVED - No Ads disabled
            // dailyGift_Plinko.Button.onClick.AddListener(DailyGift_PlinkoGame);
            coinsPanel.AddButton.onClick.AddListener(AddCoinsButton);
            playButton.onClick.AddListener(PlayButton);
            gamePlayButton.onClick.AddListener(TapToPlayResetButton);
            profileButton.onClick.AddListener(ProfileOpenButton);
            profileBtn_Second.onClick.AddListener(ProfileOpenButton);
            leaderBoardButton.Button.onClick.AddListener(LeaderBoardButton);

            if (privacyYesButton != null) privacyYesButton.onClick.AddListener(OnPrivacyYesClicked);
            if (privacyNoButton != null) privacyNoButton.onClick.AddListener(OnPrivacyNoClicked);
            if (privacyPolicyButton != null) privacyPolicyButton.onClick.AddListener(OnPrivacyPolicyClicked);

            // Auto-find auth manager if not assigned
            if (authManager == null)
                authManager = FindObjectOfType<CrystalLoginAuthManager>();

            // Hide login text initially
            if (pleaseLoginText != null)
                pleaseLoginText.gameObject.SetActive(false);

            // Update leaderboard button based on login status
            UpdateLeaderboardButtonState();

            SafeAreaHandler.RegisterRectTransform(safeAreaRectTransform);
        }

        public void TapToPlayResetButton()
        {
            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif
            MyAdsAdapter.HideBanner();
            HidePlayButton();
            HideGameLogo();
            levelGrid.ShowLevelGrid();
        }

        #region Show/Hide

        public override void PlayShowAnimation()
        {
            showHideStoreAdButtonDelayTweenCase?.Kill();

            HideAdButton(true);
            // iapStoreButton.Hide(true); // IAP Disabled!
            // DON'T hide ALL bottom buttons - our custom animation handles their visibility
            // settingButton.Hide(true); // Animated separately
            // dailyBonusButton.Hide(true); // Animated separately
            // skinsStoreButton.Hide(true); // Animated separately
            // dailyGift_Plinko.Hide(true); // Animated separately
            // leaderBoardButton.Hide(true); // Animated separately
            ShowTapToPlay();
            ShowPlayButton();
            ShowGameLogo();
            StartSimpleSpin();
            // coinsLabelScalable.Show(); // Animation removed

            CrystalUILevelNumberText.Show();
            playButtonText.text = "LEVEL " + (CrystalLevelController.MaxReachedLevelIndex + 1);

            // Hide level grid when main menu opens (will show when play button is clicked)
            if (levelGrid != null && levelGrid.canvas != null)
            {
                levelGrid.canvas.enabled = false;
                Debug.Log("[MainMenu] Grid Panel hidden on startup");
            }

            // Update leaderboard button state on show
            UpdateLeaderboardButtonState();

            // Animate top bar and bottom buttons
            AnimateTopBar();
            AnimateBottomButtons();

            // Show remaining buttons (only ad button now, all bottom buttons are animated)
            showHideStoreAdButtonDelayTweenCase = Tween.DelayedCall(0.5f, delegate
            {
                ShowAdButton();
            });

            CrystalMapLevelAbstractBehavior.OnLevelClicked += OnLevelOnMapSelected;

            ScreenManager.OnPageOpened(this);
            StartCoroutine(ShineLoop());

            ShowPrivacyPopupIfNeeded();
        }

        private IEnumerator ShineLoop()
        {
            while (true)
            {
                foreach (var shiny in shinies)
                {
                    if (shiny != null && shiny.gameObject.activeInHierarchy)
                    {
                        shiny.Play(1f);

                        // Only use the built-in shiny effect (no scaling/fading helpers)
                    }
                }

                yield return new WaitForSeconds(4f);
            }
        }

        private void ShowPrivacyPopupIfNeeded()
        {
            if (Application.platform != RuntimePlatform.Android)
                return;

            if (PlayerPrefs.GetInt(PRIVACY_ACCEPTED_KEY, 0) == 1)
                return;

            privacyBG.SetActive(true);
            if (privacyPopupRoot != null)
            {
                privacyPopupRoot.SetActive(true);
                privacyPopupRoot.transform.localScale = Vector3.zero;
                privacyPopupRoot.transform.DOScale(Vector3.one, 0.35f);
            }
            else
            {
                Debug.LogWarning("[CrystalUIMainMenu] privacyPopupRoot is not assigned in inspector.");
            }
        }

        private void HidePrivacyPopupImmediate()
        {
            if (privacyPopupRoot == null) return;
            privacyPopupRoot.transform.DOKill();
            privacyPopupRoot.transform.localScale = Vector3.zero;
            privacyPopupRoot.SetActive(false);
        }

        private void HidePrivacyPopupAnimated(System.Action onComplete = null)
        {
            if (privacyPopupRoot == null)
            {
                onComplete?.Invoke();
                return;
            }

            privacyPopupRoot.transform.DOScale(Vector3.zero, 0.22f).OnComplete(() =>
            {
                privacyPopupRoot.SetActive(false);
                privacyBG.SetActive(false);
                onComplete?.Invoke();
            });
        }

        private void OnPrivacyYesClicked()
        {
            PlayerPrefs.SetInt(PRIVACY_ACCEPTED_KEY, 1);
            PlayerPrefs.Save();

            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif
            HidePrivacyPopupAnimated();
        }

        private void OnPrivacyNoClicked()
        {
            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif
            HidePrivacyPopupAnimated();
        }

        private void OnPrivacyPolicyClicked()
        {
            if (!string.IsNullOrEmpty(privacyPolicyUrl))
            {
                Application.OpenURL(privacyPolicyUrl);
            }
            else
            {
                Debug.LogWarning("[CrystalUIMainMenu] privacyPolicyUrl is empty.");
            }
        }

        public void Apply(PlayerData p)
        {
            if (p == null)
            {
                ApplyGuest();
                return;
            }

            var name = string.IsNullOrEmpty(p.name) ? "Guest" : p.name;
            if (nameLabel) nameLabel.text = name;

            if (p.photoUrl != null && p.photoUrl.StartsWith("http"))
            {
                if (_photoLoadRoutine != null && _currentPhotoUrl != p.photoUrl)
                {
                    StopCoroutine(_photoLoadRoutine);
                    _photoLoadRoutine = null;
                }
                _currentPhotoUrl = p.photoUrl;
                if (_photoLoadRoutine == null)
                    _photoLoadRoutine = StartCoroutine(LoadPhotoThenApply(_currentPhotoUrl));
                return;
            }

            if (avatarImage && avatarSprites != null && avatarSprites.Length > 0)
            {
                int id = Mathf.Clamp(p.avatarId, 0, avatarSprites.Length - 1);
                avatarImage.sprite = avatarSprites[id];
            }
        }

        private IEnumerator LoadPhotoThenApply(string url)
        {
            using (var req = UnityWebRequestTexture.GetTexture(url))
            {
                yield return req.SendWebRequest();
#if UNITY_2020_2_OR_NEWER
                bool ok = req.result == UnityWebRequest.Result.Success;
#else
                bool ok = !(req.isNetworkError || req.isHttpError);
#endif
                if (ok)
                {
                    var tex = DownloadHandlerTexture.GetContent(req);
                    if (tex != null && avatarImage)
                    {
                        var rect = new Rect(0, 0, tex.width, tex.height);
                        var sprite = Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f), 100f);
                        avatarImage.sprite = sprite;
                    }
                }
            }
            _photoLoadRoutine = null;
        }

        public void ApplyGuest()
        {
            string name = PlayerPrefs.GetString(GUEST_NAME_KEY, "Guest");
            int avatar = PlayerPrefs.GetInt(GUEST_AVATAR_KEY, 0);

            if (nameLabel) nameLabel.text = name;

            if (avatarImage && avatarSprites != null && avatarSprites.Length > 0)
            {
                int id = Mathf.Clamp(avatar, 0, avatarSprites.Length - 1);
                avatarImage.sprite = avatarSprites[id];
            }
        }

        public override void PlayShowAnimationMainReturn()
        {
            showHideStoreAdButtonDelayTweenCase?.Kill();

            HideAdButton(true);
            // iapStoreButton.Hide(true); // IAP Disabled!
            // DON'T hide ALL bottom buttons - our custom animation handles their visibility
            // settingButton.Hide(true); // Animated separately
            // dailyBonusButton.Hide(true); // Animated separately
            // skinsStoreButton.Hide(true); // Animated separately
            // dailyGift_Plinko.Hide(true); // Animated separately
            // leaderBoardButton.Hide(true); // Animated separately
            HideTapToPlayButton(true);
            HidePlayButton(true);
            HideGameLogo(true);
            // coinsLabelScalable.Show(); // Animation removed

            CrystalUILevelNumberText.Show();
            playButtonText.text = "LEVEL " + (CrystalLevelController.MaxReachedLevelIndex + 1);

            // Show and regenerate grid when returning from game
            if (levelGrid != null)
            {
                levelGrid.ShowGridOnMainMenu();
                Debug.Log("[MainMenu] Showing Grid Panel when returning from game");
            }

            // Reset button positions first (in case they were moved off-screen)
            //   ResetBottomButtonPositions();

            // Animate top bar & bottom buttons when returning
            AnimateTopBar();
            AnimateBottomButtons();

            showHideStoreAdButtonDelayTweenCase = Tween.DelayedCall(0.5f, delegate
            {
                ShowAdButton();
            });

            CrystalMapLevelAbstractBehavior.OnLevelClicked += OnLevelOnMapSelected;

            ScreenManager.OnPageOpened(this);
        }

        public override void PlayHideAnimation()
        {
            if (!isPageDisplayed)
                return;

            showHideStoreAdButtonDelayTweenCase?.Kill();

            isPageDisplayed = false;

            HideTapToPlayButton();
            HidePlayButton();
            HideGameLogo();
            // coinsLabelScalable.Hide(); // Animation removed

            // Hide level grid canvas when main menu closes
            if (levelGrid != null && levelGrid.canvas != null)
            {
                levelGrid.canvas.enabled = false;
                Debug.Log("[MainMenu] Disabled GridPanel canvas");
            }

            HideAdButton();

            /* showHideStoreAdButtonDelayTweenCase = Tween.DelayedCall(0.1f, delegate
             {
                 // iapStoreButton.Hide(); // IAP Disabled!
                 dailyBonusButton.Hide();
                 //   dailyGift_Plinko.Hide();
                 settingButton.Hide();
                 // leaderBoardButton.Hide();
             });*/

            CrystalMapLevelAbstractBehavior.OnLevelClicked -= OnLevelOnMapSelected;

            Tween.DelayedCall(0.3f, delegate
            {
                ScreenManager.OnPageClosed(this);
            });
        }

        #endregion

        #region Tap To Play Label

        public void ShowTapToPlay(bool immediately = false)
        {
            if (tapToPlayPingPong != null && tapToPlayPingPong.IsActive)
                tapToPlayPingPong.Kill();

            if (immediately)
            {
                tapToPlayRect.localScale = Vector3.one;

                tapToPlayPingPong = tapToPlayRect.transform.DOPingPongScale(1.0f, 1.05f, 0.9f,
                    Ease.Type.QuadIn, Ease.Type.QuadOut, unscaledTime: true);

                return;
            }

            tapToPlayRect.localScale = Vector3.zero;

            tapToPlayRect.DOPushScale(Vector3.one * 1.2f, Vector3.one, 0.35f, 0.2f,
                Ease.Type.CubicOut, Ease.Type.CubicIn).OnComplete(delegate
                {
                    tapToPlayPingPong = tapToPlayRect.transform.DOPingPongScale(1.0f, 1.05f, 0.9f,
                        Ease.Type.QuadIn, Ease.Type.QuadOut, unscaledTime: true);
                });
        }

        public void HideTapToPlayButton(bool immediately = false)
        {
            if (tapToPlayPingPong != null && tapToPlayPingPong.IsActive)
                tapToPlayPingPong.Kill();

            if (immediately)
            {
                tapToPlayRect.localScale = Vector3.zero;
                return;
            }

            tapToPlayRect.DOScale(Vector3.zero, 0.3f).SetEasing(Ease.Type.CubicIn);
        }

        #endregion

        #region PlayButton

        public void ShowPlayButton(bool immediately = false)
        {
            RectTransform target = GetPlayButtonRect();
            if (target == null) return;

            if (immediately)
            {
                target.localScale = Vector3.one;
                return;
            }

            target.localScale = Vector3.zero;
            DOTween.Kill(target);
            DG.Tweening.ShortcutExtensions.DOScale(target, Vector3.one, 0.3f)
                .SetEase(DG.Tweening.Ease.OutBack);
        }

        public void HidePlayButton(bool immediately = false)
        {
            RectTransform target = GetPlayButtonRect();
            if (target == null) return;

            if (immediately)
            {
                target.localScale = Vector3.zero;
                target.localRotation = Quaternion.identity;
                return;
            }

            DOTween.Kill(target);
            DG.Tweening.ShortcutExtensions.DOScale(target, Vector3.zero, 0.2f)
                .SetEase(DG.Tweening.Ease.InBack);
            target.localRotation = Quaternion.identity; // Reset rotation on hide
        }

        private void TriggerPlayButtonShrink()
        {
            RectTransform target = GetPlayButtonRect();
            if (target == null) return;

            if (playButtonHideRoutine != null)
                StopCoroutine(playButtonHideRoutine);

            playButtonHideRoutine = StartCoroutine(ShrinkPlayButtonAfterFeedback(target));
        }

        private IEnumerator ShrinkPlayButtonAfterFeedback(RectTransform target)
        {
            yield return new WaitForSeconds(0.32f);

            if (target != null)
            {
                DOTween.Kill(target);
                DG.Tweening.ShortcutExtensions.DOScale(target, Vector3.zero, 0.22f)
                    .SetEase(DG.Tweening.Ease.InBack);
            }

            if (playButton != null)
                playButton.interactable = false;

            playButtonHideRoutine = null;
        }

        private RectTransform GetPlayButtonRect()
        {
            if (gamePlayRect != null)
                return gamePlayRect;

            if (playButton != null)
                return playButton.transform as RectTransform;

            return null;
        }

        #endregion

        #region GameLogo

        public void ShowGameLogo(bool immediately = false)
        {
            if (gameLogoPingPong != null && gameLogoPingPong.IsActive)
                gameLogoPingPong.Kill();

            if (immediately)
            {
                gameLogoRect.localScale = Vector3.one;

                gameLogoPingPong = gameLogoRect.transform.DOPingPongScale(1.0f, 1.05f, 0.9f,
                    Ease.Type.QuadIn, Ease.Type.QuadOut, unscaledTime: true);

                return;
            }

            // Simple fade + scale
            gameLogoRect.localScale = Vector3.zero;

            Tween.DelayedCall(0.03f, () =>
            {
                gameLogoRect.DOScale(Vector3.one, 0.5f).SetEasing(Ease.Type.BackOut).OnComplete(delegate
                {
                    gameLogoPingPong = gameLogoRect.transform.DOPingPongScale(1.0f, 1.05f, 0.9f,
                        Ease.Type.QuadIn, Ease.Type.QuadOut, unscaledTime: true);
                });
            });
        }

        public void HideGameLogo(bool immediately = false)
        {
            if (gameLogoPingPong != null && gameLogoPingPong.IsActive)
                gameLogoPingPong.Kill();

            if (immediately)
            {
                gameLogoRect.localScale = Vector3.zero;
                return;
            }

            gameLogoRect.DOScale(Vector3.zero, 0.3f).SetEasing(Ease.Type.CubicIn);
        }

        #endregion

        #region Simple Spin

        private void StartSimpleSpin()
        {
            if (spinImageRect == null) return;

            spinImageRect.DOLocalRotate(new Vector3(0, 0, 720f), 2.0f).SetEasing(Ease.Type.QuadOut);
        }

        #endregion

        #region Top Bar + Bottom Buttons

        private void AnimateTopBar()
        {
            float startDelay = 0.1f;
            float staggerDelay = 0.06f;
            float duration = 0.4f;

            // Profile
            if (profilePanel != null)
                AnimateTopElement(profilePanel, startDelay, duration);

            // Lives
            if (livesPanel != null)
                AnimateTopElement(livesPanel, startDelay + staggerDelay, duration);

            // Coins
            if (coinsPanel != null)
            {
                RectTransform coinRect = coinsPanel.transform as RectTransform;
                if (coinRect != null)
                    AnimateTopElement(coinRect, startDelay + (staggerDelay * 2), duration);
            }
        }

        private void AnimateTopElement(RectTransform element, float delay, float duration)
        {
            if (element == null) return;

            Vector3 originalPos = element.anchoredPosition;

            element.anchoredPosition = new Vector3(originalPos.x, originalPos.y + 50f, 0);
            element.localScale = Vector3.zero;

            element.DOAnchorPos(originalPos, duration).SetDelay(delay).SetEase(DG.Tweening.Ease.OutCubic);
            element.DOScale(Vector3.one, duration).SetDelay(delay).SetEasing(Ease.Type.CubicOut);
        }

        private void ResetBottomButtonPositions()
        {
            RestoreButton(settingButton);
            RestoreButton(dailyBonusButton);
        }

        private void RestoreButton(CrystalUIMainMenuButton button)
        {
            if (button == null || button.Button == null) return;

            RectTransform buttonRect = button.Button.transform as RectTransform;
            if (buttonRect == null) return;

            button.Show(true);
            buttonRect.localScale = Vector3.one;
            StopBottomButtonIdle(buttonRect);
        }

        private void AnimateBottomButtons()
        {
            AnimateBottomButton(settingButton, 0f);
            AnimateBottomButton(dailyBonusButton, 0.08f);
        }

        private void AnimateBottomButton(CrystalUIMainMenuButton button, float delay)
        {
            if (button == null || button.Button == null) return;

            RectTransform rect = button.Button.transform as RectTransform;
            if (rect == null) return;

            button.Show(true);
            rect.gameObject.SetActive(true);
            StopBottomButtonIdle(rect);

            rect.localScale = Vector3.one * 0.55f;

            Sequence seq = DOTween.Sequence();
            seq.SetDelay(delay);
            seq.Append(DG.Tweening.ShortcutExtensions.DOScale(rect, Vector3.one * 1.08f, 0.25f)
                .SetEase(DG.Tweening.Ease.OutBack));
            seq.Append(DG.Tweening.ShortcutExtensions.DOScale(rect, Vector3.one, 0.18f)
                .SetEase(DG.Tweening.Ease.InOutSine));
            seq.OnComplete(() => StartBottomButtonIdle(rect));
        }

        private void StartBottomButtonIdle(RectTransform rect)
        {
            StopBottomButtonIdle(rect);

            DG.Tweening.Tweener idleTween = DG.Tweening.ShortcutExtensions.DOScale(rect, Vector3.one * 1.03f, 0.75f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(DG.Tweening.Ease.InOutSine);

            bottomButtonIdleTweens[rect] = idleTween;
        }

        private void StopBottomButtonIdle(RectTransform rect)
        {
            if (rect != null && bottomButtonIdleTweens.TryGetValue(rect, out DG.Tweening.Tweener tween))
            {
                tween?.Kill();
                bottomButtonIdleTweens.Remove(rect);
            }
        }

        private void StopAllBottomButtonIdles()
        {
            foreach (KeyValuePair<RectTransform, DG.Tweening.Tweener> pair in bottomButtonIdleTweens)
            {
                pair.Value?.Kill();
            }

            bottomButtonIdleTweens.Clear();
        }

        #endregion

        #region Ad Button

        private void ShowAdButton(bool immediately = false)
        {
            // IAP REMOVED - No Ads button permanently hidden
            if (noAdsButton != null && noAdsButton.Button != null)
            {
                noAdsButton.Button.gameObject.SetActive(false);
            }
        }

        private void HideAdButton(bool immediately = false)
        {
            // IAP REMOVED - No Ads button permanently hidden
            if (noAdsButton != null && noAdsButton.Button != null)
            {
                noAdsButton.Button.gameObject.SetActive(false);
            }
        }

        #endregion

        #region Buttons Logic

        private void PlayButton()
        {
            print("IN");
            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif
            TriggerPlayButtonShrink();

            Debug.Log("[MainMenu] Play button clicked - loading current level!");

            OnPlayTriggered(CrystalLevelController.MaxReachedLevelIndex);
        }

        private void GamePlayButton()
        {
            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif
        }

        private void OnLevelOnMapSelected(int levelId)
        {
            OnPlayTriggered(levelId);
        }

        private void OnPlayTriggered(int levelId)
        {
            if (CrystalLivesSystem.Lives > 0 || CrystalLivesSystem.InfiniteMode)
            {
                ScreenOverlay.Show(0.3f, () =>
                {
                    ScreenManager.DisableScreen<CrystalUIMainMenu>();
                    CrystalGameManager.LoadLevel(levelId);
                    ScreenOverlay.Hide(0.3f);
                }, true);
            }
            else
            {
                CrystalUIAddLivesPanel.Show((bool lifeAdded) =>
                {
                    if (lifeAdded)
                    {
                        ScreenOverlay.Show(0.3f, () =>
                        {
                            ScreenManager.DisableScreen<CrystalUIMainMenu>();
                            CrystalGameManager.LoadLevel(levelId);
                            ScreenOverlay.Hide(0.3f);
                        }, true);
                    }
                    else
                    {
                        Debug.Log("[MainMenu] User closed lives panel - showing Grid Panel");

                        if (levelGrid != null)
                        {
                            HidePlayButton(true);
                            HideGameLogo(true);
                            HideTapToPlayButton(true);

                            levelGrid.ShowLevelGrid();

                            Debug.Log("[MainMenu] Grid Panel shown with animation after lives panel closed");
                        }
                        else
                        {
                            Debug.LogError("[MainMenu] levelGrid is NULL!");
                        }
                    }
                });
            }
        }

        public CrystalUIProfilePage UIProfilePage;

        public void ProfileOpenButton()
        {
            var profilePage = ScreenManager.GetPage<CrystalUIProfilePage>();
            if (profilePage == null)
            {
                Debug.LogError("[CrystalUIMainMenu] Profile page not found!");
                return;
            }

            if (profilePage.IsPageDisplayed)
                return;

            ScreenManager.CloseScreen<CrystalUIMainMenu>();
            ScreenManager.DisplayScreen<CrystalUIProfilePage>();

            if (UIProfilePage != null)
                UIProfilePage.RefreshNow();

            ScreenManager.PageClosed += OnProfilePanelClose;
            CrystalMapBehavior.DisableScroll();
            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif
        }

        private void DailyBonusButton()
        {
            if (ScreenManager.GetPage<CrystalUIDailyBonus>().IsPageDisplayed)
                return;

            CrystalUILevelNumberText.Hide(true);
            ScreenManager.CloseScreen<CrystalUIMainMenu>();
            ScreenManager.DisplayScreen<CrystalUIDailyBonus>();
            ScreenManager.PageClosed += OnDailyBonusClosed;
            CrystalMapBehavior.DisableScroll();

            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif
        }

        private void DailyGift_PlinkoGame()
        {
            if (ScreenManager.GetPage<CrystalUIPlinko>().IsPageDisplayed)
                return;

            ScreenManager.CloseScreen<CrystalUIMainMenu>();
            ScreenManager.DisplayScreen<CrystalUIPlinko>();
            CrystalMapBehavior.DisableScroll();

            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif
        }

        private void SkinsStoreButton()
        {
            if (ScreenManager.GetPage<CrystalUISkinStore>().IsPageDisplayed)
                return;

            CrystalUILevelNumberText.Hide(true);
            ScreenManager.CloseScreen<CrystalUIMainMenu>();
            ScreenManager.DisplayScreen<CrystalUISkinStore>();
            ScreenManager.PageClosed += OnSkinStoreClosed;
            CrystalMapBehavior.DisableScroll();

            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif
        }

        private void LeaderBoardButton()
        {
            bool isLoggedIn = authManager != null && authManager.CurrentUser != null;

            if (!isLoggedIn)
            {
                SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
                ShowPleaseLoginMessage();
                return;
            }

            if (ScreenManager.GetPage<CrystalUILeaderBoard>().IsPageDisplayed)
                return;

            CrystalUILevelNumberText.Hide(true);
            ScreenManager.CloseScreen<CrystalUIMainMenu>();
            ScreenManager.DisplayScreen<CrystalUILeaderBoard>();
            ScreenManager.PageClosed += OnLeaderBoardClosed;
            CrystalMapBehavior.DisableScroll();

            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif
        }

        private void ShowPleaseLoginMessage()
        {
            if (pleaseLoginText == null) return;

            StopCoroutine(nameof(HidePleaseLoginTextAfterDelay));

            pleaseLoginText.gameObject.SetActive(true);

            StartCoroutine(HidePleaseLoginTextAfterDelay());
        }

        private IEnumerator HidePleaseLoginTextAfterDelay()
        {
            yield return new WaitForSeconds(2f);
            if (pleaseLoginText != null)
                pleaseLoginText.gameObject.SetActive(false);
        }

        private void UpdateLeaderboardButtonState()
        {
            if (leaderBoardButton.Button == null) return;

            Image buttonImage = leaderBoardButton.Button.GetComponent<Image>();
            if (buttonImage == null) return;

            bool isLoggedIn = authManager != null && authManager.CurrentUser != null;

            if (isLoggedIn)
            {
                if (leaderboardUnlockedSprite != null)
                    buttonImage.sprite = leaderboardUnlockedSprite;
            }
            else
            {
                if (leaderboardLockedSprite != null)
                    buttonImage.sprite = leaderboardLockedSprite;
            }
        }

        private void OnSkinStoreClosed(BaseScreen page, System.Type pageType)
        {
            if (pageType.Equals(typeof(CrystalUISkinStore)))
            {
                ScreenManager.PageClosed -= OnSkinStoreClosed;
                CrystalMapBehavior.EnableScroll();
                ScreenManager.DisplayScreenReturn<CrystalUIMainMenu>();
                MyAdsAdapter.HideBanner();
            }
        }

        private void OnDailyBonusClosed(BaseScreen page, System.Type pageType)
        {
            if (pageType.Equals(typeof(CrystalUIDailyBonus)))
            {
                ScreenManager.PageClosed -= OnDailyBonusClosed;
                CrystalMapBehavior.EnableScroll();
                ScreenManager.DisplayScreenReturn<CrystalUIMainMenu>();
                MyAdsAdapter.HideBanner();
            }
        }

        private void OnDailySpinClosed(BaseScreen page, System.Type pageType)
        {
            if (pageType.Equals(typeof(CrystalUIDailySpin)))
            {
                ScreenManager.PageClosed -= OnDailySpinClosed;
                CrystalMapBehavior.EnableScroll();
                ScreenManager.DisplayScreenReturn<CrystalUIMainMenu>();
                MyAdsAdapter.HideBanner();
            }
        }

        private void OnProfilePanelClose(BaseScreen page, System.Type pageType)
        {
            if (pageType.Equals(typeof(CrystalUIProfilePage)))
            {
                ScreenManager.PageClosed -= OnProfilePanelClose;
                CrystalMapBehavior.EnableScroll();
                ScreenManager.DisplayScreenReturn<CrystalUIMainMenu>();
                MyAdsAdapter.HideBanner();
            }
        }

        private void OnLeaderBoardClosed(BaseScreen page, System.Type pageType)
        {
            if (pageType.Equals(typeof(CrystalUILeaderBoard)))
            {
                ScreenManager.PageClosed -= OnLeaderBoardClosed;
                CrystalMapBehavior.EnableScroll();
                ScreenManager.DisplayScreenReturn<CrystalUIMainMenu>();
                MyAdsAdapter.HideBanner();
            }
        }

        // IAP REMOVED - No Ads button functionality disabled
        private void NoAdButton()
        {
            Debug.Log("[CrystalUIMainMenu] No Ads feature has been removed from this version.");
            return;
        }

        private void AddCoinsButton()
        {
            // IAPStoreButton(); // IAP Disabled! - No coin purchase
            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif
        }

        #endregion
    }
}