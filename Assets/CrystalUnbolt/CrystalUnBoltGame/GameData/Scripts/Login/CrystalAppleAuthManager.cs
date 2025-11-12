using System;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;

#if UNITY_IOS
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Extensions;
using AppleAuth.Interfaces;
using AppleAuth.Native;
#endif

namespace CrystalUnbolt
{
    public class CrystalAppleAuthManager : MonoBehaviour
    {
        [Header("Apple Sign In Settings")]
        public bool enableAppleSignIn = true;

        [Header("Services")]
        [SerializeField] private CrystalPlayerDataService dataService;
        [SerializeField] private CrystalLoginAuthManager firebaseAuthManager;
        [SerializeField] private CrystalUISettings uiSettings;

        [Header("Header (optional)")]
        public CrystalUIMainMenu mainMenuHeader;

        public event Action<FirebaseUser> OnAppleSignedIn;
        public event Action OnAppleSignedOut;
        public event Action<string> OnAppleLog;

        private bool _isSigningIn;
        private PlayerData _lastProfile;

#if UNITY_IOS
        private IAppleAuthManager _appleAuthManager;
#endif

        private void Awake()
        {
#if UNITY_IOS
            if (enableAppleSignIn)
                InitializeAppleAuth();
#endif
        }

        private void Update()
        {
#if UNITY_IOS
            _appleAuthManager?.Update();
#endif
        }

#if UNITY_IOS
        private void InitializeAppleAuth()
        {
            if (AppleAuth.AppleAuthManager.IsCurrentPlatformSupported)
            {
                _appleAuthManager = new AppleAuth.AppleAuthManager(new PayloadDeserializer());
                Log("Apple Auth initialized successfully");
            }
            else
            {
                Log("Apple Sign In not supported on this device");
                enableAppleSignIn = false;
            }
        }
#endif

        public async Task SignInWithApple()
        {
#if !UNITY_IOS
            Log("Apple Sign In only supported on iOS");
            return;
#else
            if (!enableAppleSignIn)
            {
                Log("Apple Sign In disabled");
                return;
            }

            if (_isSigningIn)
            {
                Log("Already signing in");
                return;
            }

            if (_appleAuthManager == null)
            {
                Log("Apple Auth Manager not initialized");
                return;
            }

            _isSigningIn = true;
            UpdateUI();

            try
            {
                string nonce = GenerateNonce(32);
                string hashedNonce = Sha256(nonce);

                var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName, nonce: hashedNonce);

                _appleAuthManager.LoginWithAppleId(
                    loginArgs,
                    async credential =>
                    {
                        if (credential is IAppleIDCredential appleIdCredential)
                        {
                            await HandleAppleSignInSuccess(appleIdCredential, nonce);
                        }
                        else
                        {
                            Log("Invalid Apple credential");
                        }

                        _isSigningIn = false;
                        UpdateUI();
                    },
                    error =>
                    {
                        Log("Apple Sign In Error: " + error);
                        _isSigningIn = false;
                        UpdateUI();
                    }
                );
            }
            catch (Exception ex)
            {
                Log("Apple Sign In Exception: " + ex.Message);
                _isSigningIn = false;
            }
#endif
        }

#if UNITY_IOS
        private async Task HandleAppleSignInSuccess(IAppleIDCredential credential, string rawNonce)
        {
            try
            {
                string idToken = System.Text.Encoding.UTF8.GetString(credential.IdentityToken);
                string authCode = System.Text.Encoding.UTF8.GetString(credential.AuthorizationCode);

                var firebaseCred = OAuthProvider.GetCredential("apple.com", idToken, rawNonce, authCode);
                var firebaseAuth = FirebaseAuth.DefaultInstance;

                var result = await firebaseAuth.SignInWithCredentialAsync(firebaseCred);
                await result.ReloadAsync();
                var firebaseUser = firebaseAuth.CurrentUser;

                if (firebaseUser == null)
                {
                    Log("Firebase user null after sign-in");
                    return;
                }

                string email = credential.Email ?? firebaseUser.Email ?? $"apple_{firebaseUser.UserId}@privaterelay.appleid.com";
                string displayName = credential.FullName?.GivenName ?? "Apple User";

                PlayerData ensured = await dataService.EnsureProfile(firebaseUser, displayName, email, "", "", null);
                _lastProfile = ensured;

                firebaseAuthManager?.SetProfile(ensured);
                
                // Sync local progress to cloud after login
                CrystalCloudProgressSync.Inject(firebaseAuthManager, dataService);
                CrystalCloudProgressSync.ApplyCloudToLocal(ensured);
                CrystalCloudProgressSync.PushLocalToCloud();
                
                // Show success popup like Google login
                uiSettings?.OnAppleLoginSuccess();

                OnAppleSignedIn?.Invoke(firebaseUser);
                Log("Apple Sign In successful");
            }
            catch (Exception ex)
            {
                Log("Apple Sign In failed: " + ex.Message);
            }
        }
#endif

        public async Task ReauthenticateWithApple()
        {
#if !UNITY_IOS
            Log("Apple Reauth only supported on iOS");
            return;
#else
            if (_appleAuthManager == null)
            {
                Log("Apple Auth Manager not initialized");
                return;
            }

            Log("Starting Apple reauthentication...");

            string nonce = GenerateNonce(32);
            string hashedNonce = Sha256(nonce);

            var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName, nonce: hashedNonce);

            var tcs = new TaskCompletionSource<bool>();

            _appleAuthManager.LoginWithAppleId(
                loginArgs,
                async credential =>
                {
                    if (credential is IAppleIDCredential appleIdCredential)
                    {
                        await HandleAppleReauthSuccess(appleIdCredential, nonce);
                        tcs.SetResult(true);
                    }
                    else
                    {
                        tcs.SetResult(false);
                    }
                },
                error =>
                {
                    Log("Reauth error: " + error);
                    tcs.SetResult(false);
                }
            );

            await tcs.Task;
#endif
        }

#if UNITY_IOS
        private async Task HandleAppleReauthSuccess(IAppleIDCredential credential, string nonce)
        {
            try
            {
                string idToken = System.Text.Encoding.UTF8.GetString(credential.IdentityToken);
                string authCode = System.Text.Encoding.UTF8.GetString(credential.AuthorizationCode);

                var firebaseCred = OAuthProvider.GetCredential("apple.com", idToken, nonce, authCode);
                var firebaseAuth = FirebaseAuth.DefaultInstance;

                var currentUser = firebaseAuth.CurrentUser;
                if (currentUser != null)
                {
                    await currentUser.ReauthenticateAsync(firebaseCred);
                    Log("Apple Reauthentication successful ✅");
                }
                else
                {
                    Log("No current Firebase user for reauth");
                }
            }
            catch (Exception ex)
            {
                Log("HandleAppleReauthSuccess error: " + ex.Message);
            }
        }
#endif

        public void SignOutFromApple()
        {
            Log("Signing out from Apple...");
            OnAppleSignedOut?.Invoke();
            _lastProfile = null;
            UpdateUI();
        }

        private void UpdateUI() { }
        private void Log(string msg) { Debug.Log("[AppleAuth] " + msg); OnAppleLog?.Invoke(msg); }

        private string GenerateNonce(int length)
        {
            const string charset = "0123456789ABCDEFGHIJKLMNOPQRSTUVXYZabcdefghijklmnopqrstuvwxyz-._";
            var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            var result = new char[length];
            var bytes = new byte[length];

            rng.GetBytes(bytes);
            for (int i = 0; i < length; i++)
                result[i] = charset[bytes[i] % charset.Length];

            return new string(result);
        }

        private string Sha256(string input)
        {
            var sha = new System.Security.Cryptography.SHA256Managed();
            var hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}
