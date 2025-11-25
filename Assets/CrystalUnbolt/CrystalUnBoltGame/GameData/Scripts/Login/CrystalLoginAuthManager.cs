using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Google;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalUnbolt
{
    /// <summary>
    /// Fresh Firebase + Google Sign-In manager that only targets Android builds.
    /// </summary>
    public class CrystalLoginAuthManager : MonoBehaviour
    {
        private const string DEFAULT_WEB_CLIENT_ID = "93565773157-hq85v1au3eh2pdqflsvl3i19ndcpo85c.apps.googleusercontent.com";

        [Header("Firebase / Google")]
        [Tooltip("Web OAuth client id (client_type=3 from Firebase console).")]
        [SerializeField] private string webClientId = DEFAULT_WEB_CLIENT_ID;
        [SerializeField] private bool autoSignInOnStart = true;

        [Header("UI")]
        [SerializeField] private Button loginButton;
        [SerializeField] private Button appleLoginButton;
        [SerializeField] private Button logoutButton;
        [SerializeField] private Button deleteButton;
        [SerializeField] private Button confirmDeleteButton;
        [SerializeField] private TextMeshProUGUI userLabel;
        [SerializeField] private CrystalUIMainMenu mainMenuHeader;
        [SerializeField] private CrystalUISettings uiSettings;

        [Header("Services")]
        [SerializeField] private CrystalPlayerDataService dataService;
        [SerializeField] private CrystalAppleAuthManager appleAuthManager;

        public FirebaseAuth Auth { get; private set; }
        public FirebaseUser CurrentUser => Auth?.CurrentUser;

        public event Action<FirebaseUser, GoogleSignInUser> OnSignedIn;
        public event Action OnSignedOut;
        public event Action<string> OnLog;

        private bool firebaseReady;
        private bool isSigningIn;
        private PlayerData cachedProfile;
        private EventHandler authListener;

        private bool SupportsGoogle =>
#if UNITY_ANDROID
            true;
#else
            false;
#endif

        private bool SupportsApple =>
#if UNITY_IOS
            true;
#else
            false;
#endif

        private void Awake()
        {
            if (string.IsNullOrWhiteSpace(webClientId))
                webClientId = DEFAULT_WEB_CLIENT_ID;
        }

        private async void Start()
        {
            WireUi();
            ApplyUiState();

            await InitializeFirebaseAsync();
            if (!firebaseReady) return;

            ApplyUiState();

            if (autoSignInOnStart && CurrentUser != null)
            {
                await WarmupExistingSession(CurrentUser);
                return;
            }

#if UNITY_ANDROID
            if (autoSignInOnStart)
                await TrySilentGoogleSignInAsync();
#endif
        }

        private void OnDestroy()
        {
            UnwireUi();

            if (Auth != null && authListener != null)
                Auth.StateChanged -= authListener;
        }

        private async Task InitializeFirebaseAsync()
        {
            if (firebaseReady)
                return;

            Log("Checking Firebase dependencies...");

            var status = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (status != DependencyStatus.Available)
            {
                Debug.LogError($"[Auth] Firebase deps missing: {status}");
                return;
            }

            Auth = FirebaseAuth.DefaultInstance;
            authListener = (_, __) => ApplyUiState();
            Auth.StateChanged += authListener;

            firebaseReady = true;
            Log("Firebase ready.");
        }

        private void WireUi()
        {
            if (loginButton)
                loginButton.onClick.AddListener(() => _ = SignInWithGoogle());

            if (appleLoginButton)
                appleLoginButton.onClick.AddListener(() => _ = SignInWithApple());

            if (logoutButton)
                logoutButton.onClick.AddListener(SignOut);

            if (deleteButton)
                deleteButton.onClick.AddListener(DeleteAccountAndReset);

            if (confirmDeleteButton)
                confirmDeleteButton.onClick.AddListener(DeleteAccountAndReset);
        }

        private void UnwireUi()
        {
            loginButton?.onClick.RemoveAllListeners();
            appleLoginButton?.onClick.RemoveAllListeners();
            logoutButton?.onClick.RemoveAllListeners();
            deleteButton?.onClick.RemoveAllListeners();
            confirmDeleteButton?.onClick.RemoveAllListeners();
        }

        public async Task SignInWithGoogle()
        {
#if !UNITY_ANDROID
            Log("Google Sign-In is only enabled for Android builds.");
            return;
#else
            if (!firebaseReady)
            {
                Log("Firebase not ready yet.");
                return;
            }

            if (isSigningIn)
                return;

            isSigningIn = true;
            ApplyUiState();

            ConfigureGoogleSignIn();
            Log("Launching Google Sign-In flow...");

            try
            {
                var gUser = await GoogleSignIn.DefaultInstance.SignIn();
                await CompleteFirebaseSignInAsync(gUser);
            }
            catch (GoogleSignIn.SignInException gex)
            {
                Debug.LogWarning($"[Auth] Google error: {gex.Status} | {gex.Message}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Auth] Sign-in failed: {ex.Message}");
            }
            finally
            {
                isSigningIn = false;
                ApplyUiState();
            }
#endif
        }

        public async Task SignInWithApple()
        {
            if (!SupportsApple)
            {
                Log("Apple Sign-In disabled on this platform.");
                return;
            }

            if (appleAuthManager != null)
                await appleAuthManager.SignInWithApple();
            else
                Debug.LogWarning("[Auth] Apple manager not assigned.");
        }

        public void SignOutFromApple()
        {
            if (!SupportsApple) return;
            appleAuthManager?.SignOutFromApple();
        }

        public void SignOut()
        {
            if (!firebaseReady)
                return;

            try { Auth?.SignOut(); } catch { }

#if UNITY_ANDROID
            try { GoogleSignIn.DefaultInstance.SignOut(); } catch { }
#endif

            cachedProfile = null;
            OnSignedOut?.Invoke();
            ApplyGuestUi();
        }

        public async void DeleteAccountAndReset()
        {
            if (!firebaseReady || Auth.CurrentUser == null)
            {
                ClearLocalProgress();
                ApplyGuestUi();
                return;
            }

            var uid = Auth.CurrentUser.UserId;

            try
            {
#if UNITY_ANDROID
                if (SupportsGoogle)
                {
                    ConfigureGoogleSignIn();
                    var gUser = await GoogleSignIn.DefaultInstance.SignInSilently();
                    if (gUser == null)
                        gUser = await GoogleSignIn.DefaultInstance.SignIn();

                    if (gUser != null && !string.IsNullOrEmpty(gUser.IdToken))
                    {
                        var cred = GoogleAuthProvider.GetCredential(gUser.IdToken, null);
                        await Auth.CurrentUser.ReauthenticateAsync(cred);
                        Log("Google re-authenticated.");
                    }
                }
#endif

#if UNITY_IOS
                if (SupportsApple && appleAuthManager != null)
                {
                    await appleAuthManager.ReauthenticateWithApple();
                    Log("Apple re-authenticated.");
                }
#endif
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Auth] Reauthentication failed: {ex.Message}");
            }

            try { await dataService.DeleteByUid(uid); } catch (Exception ex) { Debug.LogWarning($"[Auth] Profile delete failed: {ex.Message}"); }
            try { await Auth.CurrentUser.DeleteAsync(); } catch (Exception ex) { Debug.LogWarning($"[Auth] Firebase delete failed: {ex.Message}"); }

            SignOut();
            ClearLocalProgress();
            ApplyGuestUi();

            uiSettings?.DeleteDataPopUpClose();
            ScreenManager.CloseScreen<CrystalUISettings>();
        }

        public void SetProfile(PlayerData profile)
        {
            cachedProfile = profile;
            ApplyUiState();
        }

        private async Task CompleteFirebaseSignInAsync(GoogleSignInUser gUser)
        {
            if (gUser == null || string.IsNullOrEmpty(gUser.IdToken))
            {
                Log("Google user missing or no ID token returned.");
                return;
            }

            var credential = GoogleAuthProvider.GetCredential(gUser.IdToken, null);
            var result = await Auth.SignInWithCredentialAsync(credential);
            await result.ReloadAsync();

            Log($"Firebase Auth success. UID: {result.UserId}");

            await SyncPlayerProfileAsync(result, gUser);

            uiSettings?.OnGoogleLoginSuccess();
            OnSignedIn?.Invoke(result, gUser);
            ApplyUiState();
        }

        private async Task WarmupExistingSession(FirebaseUser user)
        {
            try
            {
                Log("Refreshing stored Firebase token...");
                await user.TokenAsync(true);
                await SyncPlayerProfileAsync(user, null);
                OnSignedIn?.Invoke(user, null);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Auth] Warmup failed: {ex.Message}");
            }
        }

#if UNITY_ANDROID
        private async Task TrySilentGoogleSignInAsync()
        {
            ConfigureGoogleSignIn();
            try
            {
                var silentUser = await GoogleSignIn.DefaultInstance.SignInSilently();
                if (silentUser != null)
                {
                    await CompleteFirebaseSignInAsync(silentUser);
                    return;
                }
            }
            catch (GoogleSignIn.SignInException)
            {
                // Ignore, fall back to manual sign-in.
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Auth] Silent Google sign-in skipped: {ex.Message}");
            }

            ApplyUiState();
        }
#endif

        private void ConfigureGoogleSignIn()
        {
#if UNITY_ANDROID
            GoogleSignIn.Configuration = new GoogleSignInConfiguration
            {
                WebClientId = webClientId,
                RequestEmail = true,
                RequestIdToken = true,
                RequestProfile = true,
                UseGameSignIn = false
            };

            GoogleSignIn.Configuration.ForceTokenRefresh = true;
#endif
        }

        private async Task SyncPlayerProfileAsync(FirebaseUser firebaseUser, GoogleSignInUser googleUser)
        {
            if (firebaseUser == null)
                return;

            if (dataService == null)
            {
                Debug.LogWarning("[Auth] PlayerDataService missing, profile sync skipped.");
                return;
            }

            PlayerData profile = null;

            try
            {
                profile = await dataService.LoadByUid(firebaseUser.UserId);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Auth] Load profile failed: {ex.Message}");
            }

            if (profile == null)
            {
                profile = BuildDefaultProfile(firebaseUser, googleUser);

                try
                {
                    await dataService.UpdateFields(firebaseUser.UserId, new Dictionary<string, object>
                    {
                        ["name"] = profile.name,
                        ["email"] = profile.email,
                        ["photoUrl"] = profile.photoUrl,
                        ["avatarId"] = profile.avatarId,
                        ["level"] = profile.level,
                        ["coins"] = profile.coins,
                        ["updatedAt"] = profile.updatedAt
                    });
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[Auth] Failed to create profile: {ex.Message}");
                }
            }

            cachedProfile = profile;

            CrystalCloudProgressSync.Inject(this, dataService);
            CrystalCloudProgressSync.ApplyCloudToLocal(cachedProfile);
            CrystalCloudProgressSync.PushLocalToCloud();

            CrystalUnbolt.Map.CrystalMapBehavior.Refresh();
        }

        private PlayerData BuildDefaultProfile(FirebaseUser firebaseUser, GoogleSignInUser googleUser)
        {
            string name = firebaseUser.DisplayName;
            string email = firebaseUser.Email;
            string photoUrl = firebaseUser.PhotoUrl != null ? firebaseUser.PhotoUrl.ToString() : null;

            if (googleUser != null)
            {
                if (!string.IsNullOrWhiteSpace(googleUser.DisplayName))
                    name = googleUser.DisplayName;

                if (!string.IsNullOrWhiteSpace(googleUser.Email))
                    email = googleUser.Email;

                if (googleUser.ImageUrl != null)
                    photoUrl = googleUser.ImageUrl.ToString();
            }

            return new PlayerData
            {
                uid = firebaseUser.UserId,
                name = ShortName(name),
                email = email ?? string.Empty,
                photoUrl = photoUrl ?? string.Empty,
                avatarId = 0,
                level = 1,
                coins = 0,
                updatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
        }

        private void ApplyGuestUi()
        {
            cachedProfile = null;
            mainMenuHeader?.ApplyGuest();
            if (userLabel != null)
                userLabel.text = "Guest";

            ApplyUiState();
        }

        private void ApplyUiState()
        {
            bool signedIn = firebaseReady && CurrentUser != null;

            if (loginButton)
            {
                loginButton.gameObject.SetActive(SupportsGoogle && !signedIn);
                loginButton.interactable = firebaseReady && !isSigningIn;
            }

            if (appleLoginButton)
            {
                appleLoginButton.gameObject.SetActive(SupportsApple && !signedIn);
                appleLoginButton.interactable = !isSigningIn;
            }

            if (logoutButton)
            {
                logoutButton.gameObject.SetActive(signedIn);
                logoutButton.interactable = !isSigningIn;
            }

            if (deleteButton)
                deleteButton.gameObject.SetActive(signedIn);

            if (confirmDeleteButton)
                confirmDeleteButton.interactable = signedIn && !isSigningIn;

            if (userLabel)
            {
                if (signedIn)
                {
                    var user = CurrentUser;
                    string name = cachedProfile?.name ?? ShortName(user.DisplayName);
                    string email = cachedProfile?.email ?? user.Email ?? string.Empty;
                    userLabel.text = string.IsNullOrEmpty(email) ? name : $"{name} <{email}>";

                    if (mainMenuHeader != null)
                    {
                        mainMenuHeader.Apply(cachedProfile ?? new PlayerData
                        {
                            uid = user.UserId,
                            name = name,
                            email = email,
                            avatarId = cachedProfile?.avatarId ?? 0,
                            photoUrl = cachedProfile?.photoUrl ?? user.PhotoUrl?.ToString() ?? string.Empty
                        });
                    }
                }
                else
                {
                    userLabel.text = "Guest";
                    mainMenuHeader?.ApplyGuest();
                }
            }
        }

        private void ClearLocalProgress()
        {
            var levelSave = DataManager.GetSaveObject<CrystalLevelSave>("level");
            levelSave.MaxReachedLevelIndex = 0;
            levelSave.DisplayLevelIndex = 0;
            levelSave.RealLevelIndex = 0;
            levelSave.IsPlayingRandomLevel = false;
            levelSave.LastPlayerLevelIndex = -1;

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

        private string ShortName(string display)
        {
            if (string.IsNullOrWhiteSpace(display)) return "Player";
            var parts = display.Split(' ');
            return parts.Length > 0 ? parts[0] : display;
        }

        private void Log(string message)
        {
            Debug.Log("[Auth] " + message);
            OnLog?.Invoke(message);
        }
    }
}
