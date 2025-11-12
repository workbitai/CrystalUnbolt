using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using CrystalUnbolt;

namespace CrystalUnbolt
{
    public class CrystalUILeaderBoard : BaseScreen
    {
        [Header("Animation")]
        [SerializeField] private RectTransform panel;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private float showDuration = 0.3f;
        [SerializeField] private float hideDuration = 0.3f;

        [Header("References")]
        [SerializeField] private RectTransform contentArea;   // ScrollView -> Viewport -> Content
        [SerializeField] private CrystalLeaderRow rowPrefab;
        [SerializeField] private Button closeButton;

        [Header("Avatars & Data")]
        [SerializeField] private List<Sprite> avatarPool;
        [SerializeField] private int topPlayersCount = 10;  // Show top 10 players

        [Header("Visual Sprites (Optional)")]
        [SerializeField] private Sprite rank1Sprite;       // Special sprite for rank 1
        [SerializeField] private Sprite rank2Sprite;       // Special sprite for rank 2
        [SerializeField] private Sprite rank3Sprite;       // Special sprite for rank 3
        [SerializeField] private Sprite defaultRankSprite; // Default rank background
        
        [Header("Avatar Background Sprites")]
        [SerializeField] private Sprite rank1AvatarBG;     // Gold avatar BG for rank 1
        [SerializeField] private Sprite rank2AvatarBG;     // Silver avatar BG for rank 2
        [SerializeField] private Sprite rank3AvatarBG;     // Bronze avatar BG for rank 3
        [SerializeField] private Sprite defaultAvatarBG;   // Default avatar BG
        
        [Header("Coin Icon Sprites")]
        [SerializeField] private Sprite rank1CoinIcon;     // Gold coin for rank 1
        [SerializeField] private Sprite rank2CoinIcon;     // Silver coin for rank 2
        [SerializeField] private Sprite rank3CoinIcon;     // Bronze coin for rank 3
        [SerializeField] private Sprite defaultCoinIcon;   // Default coin icon
        
        [Header("Row Background Sprites")]
        [SerializeField] private Sprite rank1RowBG;        // Gold row BG for rank 1
        [SerializeField] private Sprite rank2RowBG;        // Silver row BG for rank 2
        [SerializeField] private Sprite rank3RowBG;        // Bronze row BG for rank 3
        [SerializeField] private Sprite defaultRowBG;      // Default row BG

        [Header("Bottom Static Profile")]
        [SerializeField] private TMP_Text myNameText;
        [SerializeField] private TMP_Text myCoinText;
        [SerializeField] private TMP_Text myRankText;
        [SerializeField] private Image myAvatarImage;

        [Header("Services (Optional - Will auto-find if not set)")]
        [SerializeField] private CrystalLoginAuthManager authManager;
        [SerializeField] private CrystalPlayerDataService dataService;

        [Header("Debug (Editor Only)")]
        [SerializeField] private Button debugAddTestUsersButton;
        [SerializeField] private Button debugCheckFirebaseButton;

        public override void Init()
        {
            // Wire up close button if present
            if (closeButton)
                closeButton.onClick.AddListener(OnCloseButtonClicked);

            // Debug buttons (Editor only)
            #if UNITY_EDITOR
            if (debugAddTestUsersButton)
            {
                debugAddTestUsersButton.gameObject.SetActive(true);
                debugAddTestUsersButton.onClick.AddListener(() => DebugAddTestUsers());
            }
            if (debugCheckFirebaseButton)
            {
                debugCheckFirebaseButton.gameObject.SetActive(true);
                debugCheckFirebaseButton.onClick.AddListener(() => DebugCheckFirebase());
            }
            #else
            if (debugAddTestUsersButton)
                debugAddTestUsersButton.gameObject.SetActive(false);
            if (debugCheckFirebaseButton)
                debugCheckFirebaseButton.gameObject.SetActive(false);
            #endif
        }

        private async void DebugAddTestUsers()
        {
            if (dataService == null)
                dataService = FindObjectOfType<CrystalPlayerDataService>();

            if (dataService != null)
            {
                Debug.Log("[Leaderboard] Adding test users...");
                await dataService.AddTestUsersForLeaderboard(15);
                Debug.Log("[Leaderboard] Test users added! Reloading leaderboard...");
                
                // Reload leaderboard
                LoadRealLeaderboardAsync();
            }
        }

        private async void DebugCheckFirebase()
        {
            if (dataService == null)
                dataService = FindObjectOfType<CrystalPlayerDataService>();

            if (dataService != null)
            {
                Debug.Log("[Leaderboard] Checking Firebase...");
                var users = await dataService.GetTopPlayersByCoins(20);
                
                if (users.Count == 0)
                {
                    Debug.LogWarning("? NO USERS IN FIREBASE!");
                    Debug.Log("? Click 'Add Test Users' button or play levels to add data");
                }
                else
                {
                    Debug.Log($"? FOUND {users.Count} USERS:");
                    for (int i = 0; i < Mathf.Min(10, users.Count); i++)
                    {
                        Debug.Log($"  #{i+1}: {users[i].name} - {users[i].coins} coins");
                    }
                }
            }
        }

        private void OnCloseButtonClicked()
        {
            ScreenManager.CloseScreen<CrystalUILeaderBoard>();
        }

        public override void PlayShowAnimation()
        {
            // Show animation logic
            if (panel)
            {
                panel.localScale = Vector3.zero;
                panel.DOScale(Vector3.one, showDuration).SetEasing(Ease.Type.BackOut);
            }

            if (backgroundImage)
            {
                backgroundImage.SetAlpha(0);
                backgroundImage.DOFade(0.5f, showDuration);
            }

            // Load real leaderboard data from Firebase
            LoadRealLeaderboardAsync();

            // Notify UI controller
            Tween.DelayedCall(showDuration, () => ScreenManager.OnPageOpened(this));
        }

        public override void PlayHideAnimation()
        {
            // Hide animation logic
            if (panel)
            {
                panel.DOScale(Vector3.zero, hideDuration).SetEasing(Ease.Type.BackIn);
            }

            if (backgroundImage)
            {
                backgroundImage.DOFade(0, hideDuration);
            }

            // Notify UI controller after animation completes
            Tween.DelayedCall(hideDuration, () => ScreenManager.OnPageClosed(this));
        }

        public override void PlayShowAnimationMainReturn()
        {
            // Same as show animation for this page
            PlayShowAnimation();
        }

        /// <summary>
        /// Load real leaderboard data from Firebase
        /// </summary>
        private async void LoadRealLeaderboardAsync()
        {
            // Auto-find services if not assigned
            if (dataService == null)
                dataService = FindObjectOfType<CrystalPlayerDataService>();

            if (contentArea == null || rowPrefab == null || dataService == null)
            {
                Debug.LogWarning("[Leaderboard] Missing required components!");
                return;
            }

            // Clear old rows
            foreach (Transform child in contentArea)
                Destroy(child.gameObject);

            try
            {
                // Fetch top players from Firebase
                List<PlayerData> topPlayers = await dataService.GetTopPlayersByCoins(topPlayersCount);

                if (topPlayers == null || topPlayers.Count == 0)
                {
                    Debug.Log("[Leaderboard] No players found in Firebase");
                    return;
                }

                // Create rows for each player
                for (int i = 0; i < topPlayers.Count; i++)
                {
                    var player = topPlayers[i];
                    var row = Instantiate(rowPrefab, contentArea);
                    
                    int rank = i + 1;  // Rank 1, 2, 3...
                    
                    // Get avatar sprite
                    Sprite avatar = null;
                    if (avatarPool != null && avatarPool.Count > 0)
                    {
                        int avatarId = Mathf.Clamp(player.avatarId, 0, avatarPool.Count - 1);
                        avatar = avatarPool[avatarId];
                    }

                    // Get rank-specific sprites
                    Sprite rankSprite = GetRankSprite(rank);
                    Sprite avatarBG = GetAvatarBGSprite(rank);
                    Sprite coinIcon = GetCoinIconSprite(rank);
                    Sprite rowBG = GetRowBGSprite(rank);

                    // Set row data with ALL rank-specific sprites
                    row.SetData(rank, player.name, player.coins, avatar, rankSprite, avatarBG, coinIcon, rowBG);
                }

                Debug.Log($"[Leaderboard] Loaded {topPlayers.Count} players from Firebase");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Leaderboard] Error loading data: {ex.Message}");
            }

            // Set user's own profile after loading leaderboard
            await SetMyProfileAsync();
        }

        /// <summary>
        /// Get appropriate sprite for rank (special sprites for top 3)
        /// </summary>
        private Sprite GetRankSprite(int rank)
        {
            switch (rank)
            {
                case 1:
                    return rank1Sprite != null ? rank1Sprite : defaultRankSprite;
                case 2:
                    return rank2Sprite != null ? rank2Sprite : defaultRankSprite;
                case 3:
                    return rank3Sprite != null ? rank3Sprite : defaultRankSprite;
                default:
                    return defaultRankSprite;
            }
        }

        /// <summary>
        /// Get rank-specific avatar background sprite
        /// </summary>
        private Sprite GetAvatarBGSprite(int rank)
        {
            switch (rank)
            {
                case 1:
                    return rank1AvatarBG != null ? rank1AvatarBG : defaultAvatarBG;
                case 2:
                    return rank2AvatarBG != null ? rank2AvatarBG : defaultAvatarBG;
                case 3:
                    return rank3AvatarBG != null ? rank3AvatarBG : defaultAvatarBG;
                default:
                    return defaultAvatarBG;
            }
        }

        /// <summary>
        /// Get rank-specific coin icon sprite
        /// </summary>
        private Sprite GetCoinIconSprite(int rank)
        {
            switch (rank)
            {
                case 1:
                    return rank1CoinIcon != null ? rank1CoinIcon : defaultCoinIcon;
                case 2:
                    return rank2CoinIcon != null ? rank2CoinIcon : defaultCoinIcon;
                case 3:
                    return rank3CoinIcon != null ? rank3CoinIcon : defaultCoinIcon;
                default:
                    return defaultCoinIcon;
            }
        }

        /// <summary>
        /// Get rank-specific row background sprite
        /// </summary>
        private Sprite GetRowBGSprite(int rank)
        {
            switch (rank)
            {
                case 1:
                    return rank1RowBG != null ? rank1RowBG : defaultRowBG;
                case 2:
                    return rank2RowBG != null ? rank2RowBG : defaultRowBG;
                case 3:
                    return rank3RowBG != null ? rank3RowBG : defaultRowBG;
                default:
                    return defaultRowBG;
            }
        }

        /// <summary>
        /// Set current user's profile with real rank from Firebase
        /// </summary>
        private async Task SetMyProfileAsync()
        {
            // Auto-find services if not assigned
            if (authManager == null)
                authManager = FindObjectOfType<CrystalLoginAuthManager>();
            if (dataService == null)
                dataService = FindObjectOfType<CrystalPlayerDataService>();

            PlayerData profile = null;
            string userId = null;

            // Try to get logged-in user profile
            if (authManager != null && authManager.CurrentUser != null && dataService != null)
            {
                userId = authManager.CurrentUser.UserId;
                profile = await dataService.LoadByUid(userId);
            }

            // Fallback: Get guest profile from PlayerPrefs
            if (profile == null && dataService != null)
            {
                profile = dataService.GetGuest();
            }

            // Set User Name
            if (myNameText && profile != null)
            {
                myNameText.text = !string.IsNullOrEmpty(profile.name) ? profile.name : "Player";
            }

            // Set User Coins from EconomyManager
            if (myCoinText)
            {
                try
                {
                    int coins = EconomyManager.Get(CurrencyType.Coins);
                    myCoinText.text = coins.ToString("N0");
                }
                catch
                {
                    // Fallback to profile coins if EconomyManager is not initialized
                    if (profile != null)
                        myCoinText.text = profile.coins.ToString("N0");
                    else
                        myCoinText.text = "0";
                }
            }

            // Set User Avatar from avatarPool based on avatarId
            if (myAvatarImage && profile != null && avatarPool != null && avatarPool.Count > 0)
            {
                int avatarId = Mathf.Clamp(profile.avatarId, 0, avatarPool.Count - 1);
                myAvatarImage.sprite = avatarPool[avatarId];
            }

            // Set Real Rank from Firebase
            if (myRankText && dataService != null && !string.IsNullOrEmpty(userId))
            {
                try
                {
                    int rank = await dataService.GetUserRankByCoins(userId);
                    if (rank > 0)
                    {
                        // Format rank nicely
                        if (rank >= 1000)
                            myRankText.text = (rank / 1000) + "K+";
                        else
                            myRankText.text = rank.ToString();
                    }
                    else
                    {
                        myRankText.text = "-";
                    }
                }
                catch
                {
                    myRankText.text = "-";
                }
            }
            else if (myRankText)
            {
                // Guest user - show approximate rank
                myRankText.text = "-";
            }
        }
    }
}