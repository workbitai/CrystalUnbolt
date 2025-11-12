using System;
using System.Reflection;
using System.Threading.Tasks;
using Firebase;
using Firebase.Messaging;
using UnityEngine;

#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif

namespace CrystalUnbolt
{
    public class CrystalFirebaseNotificationManager : MonoBehaviour
    {
        bool _initialized;

        private async void Start()
        {
            Debug.Log("[FCM] Init�");

            var dep = await CrystalRCBootstrap.Ensure(); // single source of truth
            if (dep != DependencyStatus.Available)
            {
                Debug.LogError("[FCM] Firebase deps not available: " + dep);
                return;
            }

            await RequestRuntimePermissionIfAvailable();

            InitializeFirebase(); // hooks, auto-init via reflection, topic subscribe
            _initialized = true;

            // Try to get current token (available across versions)
            try
            {
                var token = await FirebaseMessaging.GetTokenAsync();
                Debug.Log("[FCM] FCM token: " + token);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[FCM] GetTokenAsync failed: " + ex.Message);
            }
        }

        private void OnDestroy()
        {
            if (!_initialized) return;
            try
            {
                FirebaseMessaging.MessageReceived -= OnMessageReceived;
                FirebaseMessaging.TokenReceived -= OnTokenReceived;
            }
            catch { /* ignore */ }
        }

        private void InitializeFirebase()
        {
            FirebaseMessaging.MessageReceived += OnMessageReceived;
            FirebaseMessaging.TokenReceived += OnTokenReceived;

            // ---- Auto-init ON (support old/new SDKs) ----
            // pehle new prop try karo, fail ho to old prop set karo
            if (!TrySetStaticBoolProperty(typeof(FirebaseMessaging), "AutoInitEnabled", true))
            {
                TrySetStaticBoolProperty(typeof(FirebaseMessaging), "TokenRegistrationOnInitEnabled", true);
            }

            // Optional: subscribe to a topic
            FirebaseMessaging.SubscribeAsync("all_users").ContinueWith(t =>
            {
                if (t.IsFaulted) Debug.LogWarning("[FCM] Topic subscribe failed: all_users");
                else Debug.Log("[FCM] Subscribed to topic: all_users");
            });

            Debug.Log("[FCM] Firebase Messaging ready ?");
        }


        private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Debug.Log("[FCM] MessageReceived");

            if (e.Message.Notification != null)
            {
                var title = e.Message.Notification.Title ?? "(no title)";
                var body = e.Message.Notification.Body ?? "(no body)";
                Debug.Log($"[FCM] Title: {title}");
                Debug.Log($"[FCM] Body : {body}");
                ShowLocalNotification(title, body);
            }

            if (e.Message.Data != null && e.Message.Data.Count > 0)
            {
                Debug.Log("[FCM] Data payload:");
                foreach (var kvp in e.Message.Data)
                    Debug.Log($"   {kvp.Key} = {kvp.Value}");
            }
        }

        private void OnTokenReceived(object sender, TokenReceivedEventArgs e)
        {
            Debug.Log("[FCM] Token received: " + e.Token);
            // TODO: send this token to your backend if required
        }

        // -------- Permissions (handle void/Task return via reflection) --------
        private async Task RequestRuntimePermissionIfAvailable()
        {
            try
            {
                var m = typeof(FirebaseMessaging).GetMethod("RequestPermissionAsync", BindingFlags.Public | BindingFlags.Static, null, Type.EmptyTypes, null);
                if (m != null)
                {
                    var result = m.Invoke(null, null);
                    if (result is Task task) // newer SDKs return Task / Task<AuthorizationStatus>
                        await task;          // await if available
                    Debug.Log("[FCM] RequestPermissionAsync invoked.");
                }
                else
                {
                    // Method not present in this SDK version; nothing to do.
                    Debug.Log("[FCM] RequestPermissionAsync not available in this SDK.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[FCM] RequestPermissionAsync failed: " + ex.Message);
            }
        }

        // -------- Local notification helper --------
        private void ShowLocalNotification(string title, string body)
        {
#if UNITY_ANDROID
            var channel = new AndroidNotificationChannel()
            {
                Id = "default_channel",
                Name = "Default Channel",
                Importance = Importance.Default,
                Description = "Generic notifications",
            };
            AndroidNotificationCenter.RegisterNotificationChannel(channel);

            var notification = new AndroidNotification()
            {
                Title = title,
                Text = body,
                FireTime = DateTime.Now
            };
            AndroidNotificationCenter.SendNotification(notification, "default_channel");
#else
            Debug.Log($"[LOCAL NOTIFICATION] {title}: {body}");
#endif
        }

        // -------- Reflection helper --------
        private static bool TrySetStaticBoolProperty(Type t, string propName, bool value)
        {
            try
            {
                var p = t.GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (p != null && p.CanWrite && p.PropertyType == typeof(bool))
                {
                    p.SetValue(null, value, null);
                    Debug.Log($"[FCM] Set {t.Name}.{propName} = {value}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[FCM] Failed to set {t.Name}.{propName}: {ex.Message}");
            }
            return false;
        }
    }
}
