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
        [SerializeField] CrystalUIMainMenuButton skinsStoreButton;
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
        private AnimCase gamePlayPingPong;
        private AnimCase gameLogoPingPong; 
        private AnimCase showHideStoreAdButtonDelayTweenCase;

        [SerializeField] private ShinyEffectForUGUI[] shinies;

        public Image avatarImage;
        public TextMeshProUGUI nameLabel;
        public Sprite[] avatarSprites;

        private const string GUEST_NAME_KEY = "profile_guest_name";
        private const string GUEST_AVATAR_KEY = "profile_guest_avatar";
        [BoxGroup("Buttons", "Buttons")]
        [SerializeField] Button profileButton;
        private Coroutine _photoLoadRoutine;
        private string _currentPhotoUrl;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F12)) { ScreenCapture.CaptureScreenshot("Assets/Screenshots/snap_" + System.DateTime.Now.ToString("HHmmss") + ".png"); }
        }
        
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
            dailyGift_Plinko.Init(STORE_AD_RIGHT_OFFSET_X);
            skinsStoreButton.Init(STORE_AD_RIGHT_OFFSET_X);
            dailyBonusButton.Init(STORE_AD_RIGHT_OFFSET_X);
            settingButton.Init(STORE_AD_RIGHT_OFFSET_X);
            leaderBoardButton.Init(STORE_AD_RIGHT_OFFSET_X);

            dailyBonusButton.Button.onClick.AddListener(DailyBonusButton);
            // iapStoreButton.Button.onClick.AddListener(IAPStoreButton); // IAP Disabled!
            // noAdsButton.Button.onClick.AddListener(NoAdButton); // IAP REMOVED - No Ads disabled
            dailyGift_Plinko.Button.onClick.AddListener(DailyGift_PlinkoGame);
            skinsStoreButton.Button.onClick.AddListener(SkinsStoreButton);
            coinsPanel.AddButton.onClick.AddListener(AddCoinsButton);
            playButton.onClick.AddListener(PlayButton);
            gamePlayButton.onClick.AddListener(TapToPlayResetButton);
            profileButton.onClick.AddListener(ProfileOpenButton);
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

            // Animate top bar (3 elements) and bottom buttons (4 buttons) together - simple and clean
            AnimateTopBar();
            AnimateBottomButtons();

            // Show remaining buttons (only ad button now, all bottom buttons are animated)
            showHideStoreAdButtonDelayTweenCase = Tween.DelayedCall(0.5f, delegate
            {
                ShowAdButton();
                // All bottom buttons now use custom animation - nothing else to show here
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

                        IconAnimationHelper.PlayHeartbeatShake(shiny.transform, 1.2f, 1.02f, 0f, 4f, false);
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
            ResetBottomButtonPositions();

            // Animate top bar and bottom buttons when returning
            AnimateTopBar();
            AnimateBottomButtons();

            showHideStoreAdButtonDelayTweenCase = Tween.DelayedCall(0.5f, delegate
            {
                ShowAdButton();
                // All bottom buttons now use custom animation - nothing else to show here
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

            showHideStoreAdButtonDelayTweenCase = Tween.DelayedCall(0.1f, delegate
            {
                // iapStoreButton.Hide(); // IAP Disabled!
                skinsStoreButton.Hide();
                dailyBonusButton.Hide();
                dailyGift_Plinko.Hide();
                settingButton.Hide();
               // leaderBoardButton.Hide();
            });

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
            if (gamePlayPingPong != null && gamePlayPingPong.IsActive)
                gamePlayPingPong.Kill();

            if (immediately)
            {
                gamePlayRect.localScale = Vector3.one;

                gamePlayPingPong = gamePlayRect.transform.DOPingPongScale(1.0f, 1.05f, 0.9f,
                    Ease.Type.QuadIn, Ease.Type.QuadOut, unscaledTime: true);

                return;
            }

            // SIMPLE ANIMATION: Only clockwise rotation
            gamePlayRect.localScale = Vector3.one; // Keep normal scale
            gamePlayRect.localRotation = Quaternion.Euler(0, 0, 360f); // Start rotated 360 degrees (one full rotation)

            // Rotate clockwise to normal position
            Tween.DelayedCall(0.3f, () =>
            {
                gamePlayRect.DOLocalRotate(Vector3.zero, 0.5f).SetEasing(Ease.Type.CubicOut).OnComplete(delegate
                {
                    // Start breathing pulse after rotation
                    gamePlayPingPong = gamePlayRect.transform.DOPingPongScale(1.0f, 1.05f, 0.9f,
                        Ease.Type.QuadIn, Ease.Type.QuadOut, unscaledTime: true);
                });
            });
        }

        public void HidePlayButton(bool immediately = false)
        {
            if (gamePlayPingPong != null && gamePlayPingPong.IsActive)
                gamePlayPingPong.Kill();

            if (immediately)
            {
                gamePlayRect.localScale = Vector3.zero;
                gamePlayRect.localRotation = Quaternion.identity;
                return;
            }

            gamePlayRect.DOScale(Vector3.zero, 0.3f).SetEasing(Ease.Type.CubicIn);
            gamePlayRect.localRotation = Quaternion.identity; // Reset rotation on hide
        }

        #endregion

        #region GameLogo (duplicate)

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

            // SIMPLIFIED ANIMATION: Simple fade and scale (like top/bottom UI)
            gameLogoRect.localScale = Vector3.zero;

            // Simple scale up with slight delay
            Tween.DelayedCall(0.15f, () =>
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
          //  gameBG.GetComponent<Image>().DOFade(0f, 0.5f).OnComplete(() => gameBG.SetActive(false));
        }

        #endregion

        #region Simple Spin

        private void StartSimpleSpin()
        {
            if (spinImageRect == null) return;
            
            spinImageRect.DOLocalRotate(new Vector3(0, 0, 720f), 2.0f).SetEasing(Ease.Type.QuadOut);
        }

        #endregion

        #region Top Bar and Bottom Buttons Combined Animations

        private void AnimateTopBar()
        {
            float startDelay = 0.1f;
            float staggerDelay = 0.06f;
            float duration = 0.4f;

            // TOP 3 ELEMENTS - Simple fade and scale animation
            // 1. Profile Panel (Left)
            if (profilePanel != null)
            {
                AnimateTopElement(profilePanel, startDelay, duration);
            }

            // 2. Lives Panel (Center)
            if (livesPanel != null)
            {
                AnimateTopElement(livesPanel, startDelay + staggerDelay, duration);
            }

            // 3. Coins Panel (Right)
            if (coinsPanel != null)
            {
                RectTransform coinRect = coinsPanel.transform as RectTransform;
                if (coinRect != null)
                {
                    AnimateTopElement(coinRect, startDelay + (staggerDelay * 2), duration);
                }
            }
        }

        private void AnimateTopElement(RectTransform element, float delay, float duration)
        {
            if (element == null) return;

            // Store original position
            Vector3 originalPos = element.anchoredPosition;
            
            // Start from slightly above (much less dramatic)
            element.anchoredPosition = new Vector3(originalPos.x, originalPos.y + 50f, 0);
            element.localScale = Vector3.zero;
            
            // Slide down to original position with simple easing
            element.DOAnchorPos(originalPos, duration).SetDelay(delay).SetEase(DG.Tweening.Ease.OutCubic);
            
            // Scale up smoothly without much bounce
            element.DOScale(Vector3.one, duration).SetDelay(delay).SetEasing(Ease.Type.CubicOut);
        }

        private void ResetBottomButtonPositions()
        {
            // Reset button X positions to their saved positions (but keep them invisible for animation)
            ResetButtonPosition(settingButton);
            ResetButtonPosition(dailyBonusButton);
            ResetButtonPosition(skinsStoreButton);
            ResetButtonPosition(dailyGift_Plinko);
        }

        private void ResetButtonPosition(CrystalUIMainMenuButton button)
        {
            if (button == null || button.Button == null) return;
            
            RectTransform buttonRect = button.Button.transform as RectTransform;
            if (buttonRect == null) return;

            // Reset position to saved X position immediately, but keep invisible (scale 0)
            button.Show(true); // This sets the correct X position
            buttonRect.localScale = Vector3.zero; // But keep it invisible for animation
        }

        private void AnimateBottomButtons()
        {
            float startDelay = 0.1f;
            float staggerDelay = 0.06f;
            float duration = 0.4f;

            // ALL BOTTOM BUTTONS - Simple fade and scale animation
            // 1. Settings Button (Leftmost)
            AnimateBottomButton(settingButton, startDelay, duration);
            
            // 2. Daily Bonus Button
            AnimateBottomButton(dailyBonusButton, startDelay + staggerDelay, duration);
            
            // 3. Skins Store Button
            AnimateBottomButton(skinsStoreButton, startDelay + (staggerDelay * 2), duration);
            
            // 4. Daily Gift/Plinko Button
            AnimateBottomButton(dailyGift_Plinko, startDelay + (staggerDelay * 3), duration);
        }

        private void AnimateBottomButton(CrystalUIMainMenuButton button, float delay, float duration)
        {
            if (button == null || button.Button == null) return;

            RectTransform buttonRect = button.Button.transform as RectTransform;
            if (buttonRect == null) return;

            // Make sure button is active
            if (!button.Button.gameObject.activeInHierarchy)
                button.Button.gameObject.SetActive(true);

            // Kill any existing tweens on this button first
            buttonRect.DOKill();

            // Store the target position BEFORE modifying it
            Vector2 targetPos = buttonRect.anchoredPosition;
            
            // Now set starting position (slightly below) and scale to zero
            buttonRect.anchoredPosition = new Vector2(targetPos.x, targetPos.y - 50f);
            buttonRect.localScale = Vector3.zero;
            
            // Animate both position and scale together with delay
            Tween.DelayedCall(delay, () =>
            {
                // Slide up to target position
                buttonRect.DOAnchorPos(targetPos, duration).SetEase(DG.Tweening.Ease.OutCubic);
                
                // Scale up smoothly
                buttonRect.DOScale(Vector3.one, duration).SetEasing(Ease.Type.CubicOut);
            });
        }

        #endregion

        #region Ad Button Label

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

        // IAP Purchase Handler - DISABLED (IAP Removed!)
        /*
        private void OnAdPurchased(ProductKeyType productKeyType)
        {
            if (productKeyType == ProductKeyType.NoAds)
            {
                HideAdButton(immediately: true);
            }
        }
        */

        #endregion

        #region Buttons (rest unchanged)

        private void PlayButton()
        {
            print("IN");
            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
           Haptic.Play(Haptic.HAPTIC_HARD);
#endif

            Debug.Log("[MainMenu] Play button clicked - loading current level!");
            
            // Load the current level directly (grid is already visible)
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
                        // User closed the panel without adding life - show grid
                        Debug.Log("[MainMenu] User closed lives panel - showing Grid Panel");
                        
                        if (levelGrid != null)
                        {
                            // Hide play button and logo first
                            HidePlayButton(true);
                            HideGameLogo(true);
                            HideTapToPlayButton(true);
                            
                            // Use the same method as the play button to show grid with animation
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

        // IAP Store Button - DISABLED (IAP System Removed!)
        /*
        private void IAPStoreButton()
        {
            if (ScreenManager.GetPage<CrystalUIStore>().IsPageDisplayed)
                return;

            CrystalUILevelNumberText.Hide(true);
            ScreenManager.CloseScreen<CrystalUIMainMenu>();
            ScreenManager.DisplayScreen<CrystalUIStore>();
            MyAdsAdapter.DestroyBanner();
            // ScreenManager.PageClosed += OnIapStoreClosed; // IAP Disabled!
            CrystalMapBehavior.DisableScroll();

            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
           Haptic.Play(Haptic.HAPTIC_HARD);
#endif
        }
        */
        public CrystalUIProfilePage UIProfilePage;
        private void ProfileOpenButton()
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
           // ScreenManager.PageClosed += OnDailySpinClosed;
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
            ScreenManager.PageClosed += OnSkinStoreClosed; // Re-enabled for Skin Store
            CrystalMapBehavior.DisableScroll();

            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
           Haptic.Play(Haptic.HAPTIC_HARD);
#endif
        }

        private void LeaderBoardButton()
        {
            // Check if user is logged in
            bool isLoggedIn = authManager != null && authManager.CurrentUser != null;

            if (!isLoggedIn)
            {
                // User not logged in - show "Please Login First" message
                SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
                ShowPleaseLoginMessage();
                return;
            }

            // User is logged in - open leaderboard
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

        /// <summary>
        /// Show "Please Login First" text for 2 seconds
        /// </summary>
        private void ShowPleaseLoginMessage()
        {
            if (pleaseLoginText == null) return;

            // Cancel any existing coroutine
            StopCoroutine(nameof(HidePleaseLoginTextAfterDelay));

            // Show text
            pleaseLoginText.gameObject.SetActive(true);

            // Hide after 2 seconds
            StartCoroutine(HidePleaseLoginTextAfterDelay());
        }

        private IEnumerator HidePleaseLoginTextAfterDelay()
        {
            yield return new WaitForSeconds(2f);
            if (pleaseLoginText != null)
                pleaseLoginText.gameObject.SetActive(false);
        }

        /// <summary>
        /// Update leaderboard button sprite based on login status
        /// </summary>
        private void UpdateLeaderboardButtonState()
        {
            if (leaderBoardButton.Button == null) return;

            Image buttonImage = leaderBoardButton.Button.GetComponent<Image>();
            if (buttonImage == null) return;

            bool isLoggedIn = authManager != null && authManager.CurrentUser != null;

            if (isLoggedIn)
            {
                // User logged in - show unlocked sprite
                if (leaderboardUnlockedSprite != null)
                    buttonImage.sprite = leaderboardUnlockedSprite;
            }
            else
            {
                // User not logged in - show locked sprite
                if (leaderboardLockedSprite != null)
                    buttonImage.sprite = leaderboardLockedSprite;
            }
        }

        // Skin Store Closed Handler - Re-enabled (IAP Store remains disabled)
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
            // Do nothing - IAP removed
            Debug.Log("[CrystalUIMainMenu] No Ads feature has been removed from this version.");
            return;

            /*
            try
            {
                if (noAdsPopUp != null)
                {
                    noAdsPopUp.Show();
                }
                else
                {
                    Debug.LogWarning("[CrystalUIMainMenu] noAdsPopUp is null, cannot show popup");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[CrystalUIMainMenu] Error showing No Ads popup: {e.Message}");
            }

            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
           Haptic.Play(Haptic.HAPTIC_HARD);
#endif
            */
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
