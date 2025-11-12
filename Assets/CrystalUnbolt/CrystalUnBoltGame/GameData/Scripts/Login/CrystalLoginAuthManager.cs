using System;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Google;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalUnbolt
{
    public class CrystalLoginAuthManager : MonoBehaviour
    {
        [Header("Google Sign-In Client IDs")]
        [Tooltip("Web Client ID (client_type=3 from Firebase)")]
        public string webClientId = "93565773157-hq85v1au3eh2pdqflsvl3i19ndcpo85c.apps.googleusercontent.com";

        [Header("Optional UI")]
        public Button loginButton;          
        public Button CrystalAppleLoginButton;     
        public Button logoutButton;
        public Button deleteButton;
        public Button yesDeleteButton;
        public TextMeshProUGUI userLabel;
        public CrystalUIMainMenu mainMenuHeader;
        public CrystalUISettings uiSettings;

        [Header("Services")]
        [SerializeField] private CrystalPlayerDataService dataService;
        [SerializeField] private CrystalAppleAuthManager CrystalAppleAuthManager;

        public FirebaseAuth Auth { get; private set; }
        public FirebaseUser CurrentUser => Auth?.CurrentUser;

        private bool _isSigningIn;
        private PlayerData _lastProfile;

        public event Action<FirebaseUser, GoogleSignInUser> OnSignedIn;
        public event Action OnSignedOut;
        public event Action<string> OnLog;

        private EventHandler _authStateChangedHandler;

#if UNITY_IOS
        private const bool ALLOW_APPLE = true;
        private const bool ALLOW_GOOGLE = false;
#elif UNITY_ANDROID
        private const bool ALLOW_APPLE  = false;
        private const bool ALLOW_GOOGLE = true;
#else
        // Other platforms (Standalone/Editor w/other target): hide both by default.
        private const bool ALLOW_APPLE  = false;
        private const bool ALLOW_GOOGLE = false;
#endif

        private async void Start()
        {
            Log("Initializing Firebase...");
            var dep = await CrystalRCBootstrap.Ensure();
            if (dep != DependencyStatus.Available)
            {
                Debug.LogError("[Auth] Firebase deps missing: " + dep);
                return;
            }

            Auth = FirebaseAuth.DefaultInstance;
            _authStateChangedHandler = (_, __) => UpdateUI();
            Auth.StateChanged += _authStateChangedHandler;

            WireUI();
            ApplyPlatformVisibility();  
            Log("Firebase Auth ready ?");
            UpdateUI();

            if (Auth.CurrentUser != null)
            {
                Log($"[Auth] User already signed in: {Auth.CurrentUser.UserId}");
                try
                {
                    // Force token refresh to get latest permissions
                    Log("[Auth] Refreshing auth token...");
                    await Auth.CurrentUser.TokenAsync(true);
                    Log("[Auth] Auth token refreshed successfully");
                    
                    await RefreshProfile(Auth.CurrentUser);
                    CrystalCloudProgressSync.Inject(this, dataService);
                    CrystalCloudProgressSync.ApplyCloudToLocal(_lastProfile);
                    CrystalCloudProgressSync.PushLocalToCloud();  // Sync local coins to Firebase on startup
                    OnSignedIn?.Invoke(Auth.CurrentUser, null);
                    CrystalUnbolt.Map.CrystalMapBehavior.Refresh();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[Auth] Failed to refresh profile on auto-login: {e.Message}");
                    Debug.LogError($"[Auth] Stack trace: {e.StackTrace}");
                    // Don't sign out - just update UI with what we have
                    UpdateUI();
                }
            }
        }

        private void OnDestroy()
        {
            if (Auth != null && _authStateChangedHandler != null)
                Auth.StateChanged -= _authStateChangedHandler;

            UnwireUI();
        }

        private void WireUI()
        {
            if (loginButton) loginButton.onClick.AddListener(() => _ = SignInWithGoogle());
            if (CrystalAppleLoginButton) CrystalAppleLoginButton.onClick.AddListener(() => _ = SignInWithApple());
            if (logoutButton)
            {
                logoutButton.onClick.AddListener(SignOut);
                if (yesDeleteButton) yesDeleteButton.onClick.AddListener(DeleteAccountAndReset);
            }
        }

        private void UnwireUI()
        {
            loginButton?.onClick.RemoveAllListeners();
            CrystalAppleLoginButton?.onClick.RemoveAllListeners();
            logoutButton?.onClick.RemoveAllListeners();
            deleteButton?.onClick.RemoveAllListeners();
        }

        private void ApplyPlatformVisibility()
        {
            bool signedIn = Auth != null && Auth.CurrentUser != null;

            if (loginButton)
            {
                loginButton.gameObject.SetActive(ALLOW_GOOGLE && !signedIn);
                loginButton.interactable = !_isSigningIn;
            }

            if (CrystalAppleLoginButton)
            {
                CrystalAppleLoginButton.gameObject.SetActive(ALLOW_APPLE && !signedIn);
                CrystalAppleLoginButton.interactable = !_isSigningIn;
            }
        }

        public async Task SignInWithGoogle()
        {
            if (!ALLOW_GOOGLE) { Log("Google Sign-In disabled on this platform."); return; }
            if (Auth == null || _isSigningIn) return;
            _isSigningIn = true;
            UpdateUI();

            Log("Starting Google Sign-In...");

            GoogleSignIn.Configuration = new GoogleSignInConfiguration
            {
                WebClientId = webClientId,
                RequestIdToken = true,
                RequestEmail = true,
                RequestProfile = true,
                UseGameSignIn = false
            };

            GoogleSignIn.Configuration.ForceTokenRefresh = true;

            try
            {
                var gUser = await GoogleSignIn.DefaultInstance.SignIn();
                if (gUser == null)
                {
                    Log("Google user is null");
                    return;
                }

                var cred = GoogleAuthProvider.GetCredential(gUser.IdToken, null);
                var result = await Auth.SignInWithCredentialAsync(cred);
                await result.ReloadAsync();

                Log("Firebase Auth success ? UID: " + Auth.CurrentUser.UserId);

                var ensured = await dataService.EnsureProfile(
                    Auth.CurrentUser,
                    gUser.DisplayName ?? "Player",
                    Auth.CurrentUser.Email ?? gUser.Email ?? "",
                    gUser.ImageUrl?.ToString() ?? "",
                    gUser.GivenName,
                    gUser.ImageUrl?.ToString()
                );

                _lastProfile = ensured;
                uiSettings?.OnGoogleLoginSuccess();

                CrystalCloudProgressSync.Inject(this, dataService);
                CrystalCloudProgressSync.ApplyCloudToLocal(_lastProfile);
                CrystalCloudProgressSync.PushLocalToCloud();

                CrystalUnbolt.Map.CrystalMapBehavior.Refresh();
                OnSignedIn?.Invoke(Auth.CurrentUser, gUser);
            }
            catch (GoogleSignIn.SignInException gex)
            {
                Debug.LogError($"[Auth] Google error: {gex.Status} | {gex.Message}");
            }
            catch (Exception ex)
            {
                Debug.LogError("[Auth] SignIn error: " + ex);
            }
            finally
            {
                _isSigningIn = false;
                UpdateUI();
            }
        }

        public async Task SignInWithApple()
        {
            if (!ALLOW_APPLE) { Log("Apple Sign-In disabled on this platform."); return; }
            if (CrystalAppleAuthManager != null)
                await CrystalAppleAuthManager.SignInWithApple();
            else
                Log("CrystalAppleAuthManager not assigned");
        }

        public void SignOutFromApple()
        {
            if (!ALLOW_APPLE) return;
            CrystalAppleAuthManager?.SignOutFromApple();
        }

        public void SignOut()
        {
            try { Auth?.SignOut(); } catch { }
            try { GoogleSignIn.DefaultInstance.SignOut(); } catch { }

            _lastProfile = null;
            OnSignedOut?.Invoke();
            if (mainMenuHeader) mainMenuHeader.ApplyGuest();
            if (userLabel) userLabel.text = "Guest";
            UpdateUI();
        }

        public async void DeleteAccountAndReset()
        {
            if (Auth == null || Auth.CurrentUser == null)
            {
                ClearLocalProgress();
                ApplyGuestUI();
                return;
            }

            var uid = Auth.CurrentUser.UserId;

            try
            {
                if (ALLOW_GOOGLE)
                {
                    var gUser = await GoogleSignIn.DefaultInstance.SignInSilently();
                    if (gUser == null) gUser = await GoogleSignIn.DefaultInstance.SignIn();
                    if (gUser != null && !string.IsNullOrEmpty(gUser.IdToken))
                    {
                        var cred = GoogleAuthProvider.GetCredential(gUser.IdToken, null);
                        await Auth.CurrentUser.ReauthenticateAsync(cred);
                        Log("Google reauthentication successful");
                    }
                }
                else if (ALLOW_APPLE && CrystalAppleAuthManager != null)
                {
                    await CrystalAppleAuthManager.ReauthenticateWithApple();
                    Log("Apple reauthentication completed");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Auth] Reauth failed: " + e.Message);
            }

            try { await dataService.DeleteByUid(uid); } catch (Exception e) { Debug.LogWarning("[Auth] DB delete failed: " + e.Message); }
            try { await Auth.CurrentUser.DeleteAsync(); } catch (Exception e) { Debug.LogWarning("[Auth] Auth delete failed: " + e.Message); }

            SignOut();
            ClearLocalProgress();
            ApplyGuestUI();

            uiSettings?.DeleteDataPopUpClose();
            ScreenManager.CloseScreen<CrystalUISettings>();
        }

        private async Task RefreshProfile(FirebaseUser fUser)
        {
            if (fUser == null)
            {
                Debug.LogWarning("[Auth] RefreshProfile: fUser is null");
                return;
            }
            
            if (dataService == null)
            {
                Debug.LogWarning("[Auth] RefreshProfile: dataService is null");
                return;
            }

            Debug.Log($"[Auth] RefreshProfile for UID: {fUser.UserId}");
            
            PlayerData p = null;
            
            try
            {
                p = await dataService.LoadByUid(fUser.UserId);
            }
            catch (Exception loadEx)
            {
                Debug.LogWarning($"[Auth] LoadByUid failed: {loadEx.Message}");
            }
            
            if (p == null)
            {
                Debug.Log("[Auth] No profile found, creating default profile...");
                p = new PlayerData
                {
                    uid = fUser.UserId,
                    name = ShortName(fUser.DisplayName),
                    email = fUser.Email ?? "",
                    photoUrl = fUser.PhotoUrl != null ? fUser.PhotoUrl.ToString() : "",
                    avatarId = 0,
                    level = 1,
                    coins = 0,
                    updatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };

                try
                {
                    await dataService.UpdateFields(fUser.UserId, new System.Collections.Generic.Dictionary<string, object>
                    {
                        ["name"] = p.name,
                        ["email"] = p.email,
                        ["photoUrl"] = p.photoUrl,
                        ["avatarId"] = p.avatarId,
                        ["level"] = p.level,
                        ["coins"] = p.coins,
                        ["updatedAt"] = p.updatedAt
                    });
                    Debug.Log("[Auth] Profile created successfully in database");
                }
                catch (Exception writeEx)
                {
                    Debug.LogWarning($"[Auth] Failed to create profile in database: {writeEx.Message}");
                    Debug.LogWarning("[Auth] Using local-only profile (database unavailable)");
                }
            }

            _lastProfile = p;
            UpdateUI();
        }

        private void ClearLocalProgress()
        {
            var CrystalLevelSave = DataManager.GetSaveObject<CrystalLevelSave>("level");
            CrystalLevelSave.MaxReachedLevelIndex = 0;
            CrystalLevelSave.DisplayLevelIndex = 0;
            CrystalLevelSave.RealLevelIndex = 0;
            CrystalLevelSave.IsPlayingRandomLevel = false;
            CrystalLevelSave.LastPlayerLevelIndex = -1;

            try
            {
                int coins = EconomyManager.Get(CurrencyType.Coins);
                if (coins != 0)
                {
                    if (coins > 0) EconomyManager.Substract(CurrencyType.Coins, coins);
                    else EconomyManager.Add(CurrencyType.Coins, -coins);
                }
            }
            catch { }

            try { dataService.ClearGuest(); } catch { }
            DataManager.Save(true);
        }

        private void ApplyGuestUI()
        {
            _lastProfile = null;
            if (mainMenuHeader) mainMenuHeader.ApplyGuest();
            if (userLabel) userLabel.text = "Guest";
            UpdateUI();
        }

        private void UpdateUI()
        {
            bool signedIn = Auth != null && Auth.CurrentUser != null;

            if (loginButton)
            {
                loginButton.gameObject.SetActive(ALLOW_GOOGLE && !signedIn);
                loginButton.interactable = !_isSigningIn;
            }

            if (CrystalAppleLoginButton)
            {
                CrystalAppleLoginButton.gameObject.SetActive(ALLOW_APPLE && !signedIn);
                CrystalAppleLoginButton.interactable = !_isSigningIn;
            }

            if (logoutButton)
            {
                logoutButton.gameObject.SetActive(signedIn);
                if (deleteButton) deleteButton.gameObject.SetActive(signedIn);
            }

            if (userLabel)
            {
                if (signedIn)
                {
                    var u = Auth.CurrentUser;
                    string name = _lastProfile?.name ?? u.DisplayName ?? "Guest";
                    string email = _lastProfile?.email ?? u.Email ?? "";
                    userLabel.text = $"{name} <{email}>";

                    if (mainMenuHeader)
                        mainMenuHeader.Apply(_lastProfile ?? new PlayerData
                        {
                            uid = u.UserId,
                            name = name,
                            email = email,
                            avatarId = _lastProfile?.avatarId ?? 0,
                            photoUrl = _lastProfile?.photoUrl ?? u.PhotoUrl?.ToString() ?? ""
                        });
                }
                else
                {
                    userLabel.text = "Guest";
                    mainMenuHeader?.ApplyGuest();
                }
            }
        }
        public void SetProfile(PlayerData p)
        {
            _lastProfile = p;   
            UpdateUI();         
        }
        private string ShortName(string display)
        {
            if (string.IsNullOrWhiteSpace(display)) return "Player";
            var parts = display.Split(' ');
            return parts.Length > 0 ? parts[0] : display;
        }

        private void Log(string s) { Debug.Log("[Auth] " + s); OnLog?.Invoke(s); }
    }
}
