using UnityEngine;
using System.Collections;
using Firebase;
using Firebase.Messaging;

#if UNITY_ANDROID
using UnityEngine.Android;
#elif UNITY_IOS
using Unity.Notifications.iOS;
#endif
namespace CrystalUnbolt
{
    public class CrystalNotificationManager : MonoBehaviour
    {
        private const string PermissionAskCountKey = "NotificationPermissionAskCount";

        void Start()
        {
            AskNotificationPermission();
            ClearDeliveredNotificationsOnLaunch();
        }

        void AskNotificationPermission()
        {
            int askCount = PlayerPrefs.GetInt(PermissionAskCountKey, 0);

            if (askCount >= 3)
            {
                Debug.Log("Already asked 3 times. Not asking again.");
                InitFirebase(); // Even if denied, Firebase init thai jaye for silent data messages
                return;
            }

#if UNITY_ANDROID
            if (IsAndroid13OrHigher())
            {
                if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
                {
                    Debug.Log("Requesting Android notification permission...");
                    Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
                    PlayerPrefs.SetInt(PermissionAskCountKey, askCount + 1);
                    PlayerPrefs.Save();
                }
                else
                {
                    Debug.Log("Android notification permission already granted.");
                    InitFirebase();
                }
            }
            else
            {
                InitFirebase(); // Android < 13 auto allowed
            }

#elif UNITY_IOS
        StartCoroutine(RequestIOSPermission(askCount));
#endif
        }

#if UNITY_ANDROID
        private bool IsAndroid13OrHigher()
        {
            // Avoid JNI reflection when not actually running on an Android device
            if (Application.platform != RuntimePlatform.Android) return false;
            try
            {
                using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
                {
                    int sdkInt = version.GetStatic<int>("SDK_INT");
                    return sdkInt >= 33; // Android 13+
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("IsAndroid13OrHigher check failed: " + e.Message + ". Assuming < 13.");
                return false;
            }
        }

        private void ClearDeliveredNotificationsOnLaunch()
        {
            if (Application.platform != RuntimePlatform.Android) return;
            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var contextClass = new AndroidJavaClass("android.content.Context"))
                {
                    string notifService = contextClass.GetStatic<string>("NOTIFICATION_SERVICE");
                    using (var notifManager = activity.Call<AndroidJavaObject>("getSystemService", notifService))
                    {
                        notifManager.Call("cancelAll");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Failed to clear delivered notifications: " + e.Message);
            }
        }
#else
    private bool IsAndroid13OrHigher() { return false; }
    private void ClearDeliveredNotificationsOnLaunch() { }
#endif

#if UNITY_IOS
    private IEnumerator RequestIOSPermission(int askCount)
    {
        var req = new AuthorizationRequest(
            AuthorizationOption.Alert | AuthorizationOption.Badge | AuthorizationOption.Sound, true);

        yield return new WaitUntil(() => req.IsFinished);

        if (req.Granted)
        {
            Debug.Log("iOS notification permission granted.");
            InitFirebase();
        }
        else
        {
            Debug.Log("iOS notification permission denied.");
            PlayerPrefs.SetInt(PermissionAskCountKey, askCount + 1);
            PlayerPrefs.Save();
        }
    }
#endif

        private async void InitFirebase()
        {
            var dep = await CrystalRCBootstrap.Ensure();
            if (dep == DependencyStatus.Available)
            {
                Debug.Log("Firebase Ready.");

                FirebaseMessaging.TokenReceived += OnTokenReceived;
                FirebaseMessaging.MessageReceived += OnMessageReceived;
            }
            else
            {
                Debug.LogError("Could not resolve Firebase dependencies: " + dep);
            }
        }

        private void OnTokenReceived(object sender, TokenReceivedEventArgs e)
        {
            Debug.Log("FCM Token: " + e.Token);
            // send token to your server if needed
        }

        private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (e.Message.Notification != null)
            {
                Debug.Log("Push Received: " + e.Message.Notification.Body);
            }
        }
    }
}