using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine;

namespace CrystalUnbolt
{
    [Serializable]
    public class PlayerData
    {
        public string uid;
        public string name;
        public string email;
        public string photoUrl; 
        public int avatarId;    
        public int level;
        public int coins;
        public long updatedAt;  
    }

    public class CrystalPlayerDataService : MonoBehaviour
    {
        [Header("Firebase Realtime Database")]
        [Tooltip("Firebase Console ? Realtime Database ? Database URL (e.g. https://<project-id>-default-rtdb.firebaseio.com/ OR https://<project-id>.<region>.firebasedatabase.app/)")]
        [SerializeField] private string databaseUrl = "https://crystalunbolt-sovel-default-rtdb.asia-southeast1.firebasedatabase.app/";

        [Header("Defaults for new users")]
        public int defaultLevel = 1;
        public int defaultCoins = 100;

        [Header("Guest (local) keys")]
        [SerializeField] private string guestNameKey = "profile_guest_name";
        [SerializeField] private string guestAvatarKey = "profile_guest_avatar";
        [SerializeField] private string guestLevelKey = "guest_level";
        [SerializeField] private string guestCoinsKey = "guest_coins";

        private DatabaseReference root;

        private static readonly SemaphoreSlim _initLock = new(1, 1);
        private Task<bool> _initTask;

        async void Awake()
        {
            await EnsureDatabaseAsync();
        }

       
        public Task<bool> EnsureDatabaseAsync()
        {
            if (_initTask != null) return _initTask;

            _initTask = EnsureDatabaseImpl();
            return _initTask;
        }

        private async Task<bool> EnsureDatabaseImpl()
        {
            await _initLock.WaitAsync();
            try
            {
                if (root != null) return true;

                var dep = await CrystalRCBootstrap.Ensure();
                if (dep != DependencyStatus.Available)
                {
                    Debug.LogError($"[DB] Firebase dependencies not available: {dep}");
                    root = null;
                    return false;
                }

                var app = FirebaseApp.DefaultInstance;

                string urlToUse = databaseUrl;
                if (string.IsNullOrWhiteSpace(urlToUse))
                {
                    var optUrl = app.Options != null ? app.Options.DatabaseUrl : null;
                    if (optUrl != null) urlToUse = optUrl.ToString();
                }

                if (string.IsNullOrWhiteSpace(urlToUse))
                {
                    Debug.LogError("[DB] Database URL missing. Set it in PlayerDataService.databaseUrl (Inspector) or configure it in google-services.");
                    root = null;
                    return false;
                }

                try
                {
                    var db = FirebaseDatabase.GetInstance(app, urlToUse);
                    root = db.RootReference;
                    Debug.Log($"[DB] Realtime Database ready ? ({urlToUse})");
                    Debug.Log($"[DB] Firebase App Name: {app.Name}");
                    Debug.Log($"[DB] Firebase Project ID: {app.Options.ProjectId}");
                    return true;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[DB] Failed to create Database instance with URL '{urlToUse}': {e.Message}");
                    root = null;
                    return false;
                }
            }
            finally
            {
                _initLock.Release();
            }
        }

        static long Now() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        public static string ShortNameFromDisplay(string display)
        {
            if (string.IsNullOrWhiteSpace(display)) return "Player";
            var parts = display.Trim().Split(' ');
            return parts.Length > 0 ? parts[0] : display.Trim();
        }

        public async Task<PlayerData> EnsureProfile(
      FirebaseUser fUser,
      string fbName = "Player",
      string fbEmail = "",
      string fbPhoto = "",
      string googleGivenName = null,
      string googleImageUrl = null,
      bool _unused = true 
  )
        {
            if (fUser == null) throw new Exception("EnsureProfile: FirebaseUser null");
            var ok = await EnsureDatabaseAsync();
            if (!ok) throw new Exception("EnsureProfile: Database not initialized.");

            string uid = fUser.UserId;
            var userRef = root.Child("users").Child(uid);
            var snap = await userRef.GetValueAsync();

            string rawDisplay = !string.IsNullOrEmpty(fbName) ? fbName : fUser.DisplayName;
            string shortName = !string.IsNullOrEmpty(googleGivenName)
                ? googleGivenName
                : ShortNameFromDisplay(rawDisplay);
            if (string.IsNullOrWhiteSpace(shortName)) shortName = "Player";

            string email = !string.IsNullOrEmpty(fUser.Email) ? fUser.Email : (fbEmail ?? "");
            string photoUrl = fUser.PhotoUrl != null ? fUser.PhotoUrl.ToString()
                            : (!string.IsNullOrEmpty(googleImageUrl) ? googleImageUrl : (fbPhoto ?? ""));

            PlayerData d;
            if (!snap.Exists || string.IsNullOrEmpty(snap.GetRawJsonValue()))
            {
                d = new PlayerData
                {
                    uid = uid,
                    name = shortName,
                    email = email,
                    photoUrl = photoUrl,
                    avatarId = string.IsNullOrEmpty(photoUrl) ? 0 : -1,
                    level = defaultLevel,
                    coins = defaultCoins,
                    updatedAt = Now()
                };
            }
            else
            {
                d = JsonUtility.FromJson<PlayerData>(snap.GetRawJsonValue());
                if (string.IsNullOrWhiteSpace(d.name) || d.name == "Guest" || d.name == "Player")
                    d.name = shortName;
                if (string.IsNullOrWhiteSpace(d.email))
                    d.email = email;
                if (d.avatarId < 0 && string.IsNullOrWhiteSpace(d.photoUrl) && !string.IsNullOrEmpty(photoUrl))
                    d.photoUrl = photoUrl;
                if (d.level <= 0) d.level = defaultLevel;
                if (d.coins < 0) d.coins = defaultCoins;
                d.updatedAt = Now();
            }

            await userRef.SetRawJsonValueAsync(JsonUtility.ToJson(d)); 
            return d;
        }


        public async Task<PlayerData> LoadByUid(string uid)
        {
            Debug.Log($"[DB] LoadByUid called for: {uid}");
            var ok = await EnsureDatabaseAsync();
            if (!ok)
            {
                Debug.LogError("[DB] LoadByUid: Database not initialized");
                return null;
            }
            
            try
            {
                var snap = await root.Child("users").Child(uid).GetValueAsync();
                Debug.Log($"[DB] LoadByUid snapshot exists: {snap.Exists}");
                
                if (snap.Exists && !string.IsNullOrEmpty(snap.GetRawJsonValue()))
                {
                    var data = JsonUtility.FromJson<PlayerData>(snap.GetRawJsonValue());
                    Debug.Log($"[DB] LoadByUid success: {data.name}, level: {data.level}, coins: {data.coins}");
                    return data;
                }
                
                Debug.Log($"[DB] LoadByUid: No data found for UID {uid}");
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError($"[DB] LoadByUid error: {e.Message}");
                Debug.LogError($"[DB] LoadByUid stack: {e.StackTrace}");
                return null;
            }
        }

        public async Task<PlayerData> LoadByEmail(string email)
        {
            var ok = await EnsureDatabaseAsync(); if (!ok) return null;
            var query = root.Child("users").OrderByChild("email").EqualTo(email);
            var snap = await query.GetValueAsync();
            if (!snap.Exists) return null;

            foreach (var child in snap.Children)
                return JsonUtility.FromJson<PlayerData>(child.GetRawJsonValue());

            return null;
        }

        public async Task UpdateFields(string uid, IDictionary<string, object> partial)
        {
            Debug.Log($"[DB] UpdateFields called for UID: {uid}");
            var ok = await EnsureDatabaseAsync();
            if (!ok)
            {
                Debug.LogError("[DB] UpdateFields: Database not initialized");
                return;
            }
            
            try
            {
                await root.Child("users").Child(uid).UpdateChildrenAsync(partial);
                Debug.Log($"[DB] UpdateFields success for UID: {uid}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[DB] UpdateFields error: {e.Message}");
                Debug.LogError($"[DB] UpdateFields stack: {e.StackTrace}");
                throw; // Re-throw so caller knows it failed
            }
        }

        public async Task<string> GetAsJson(string uid)
        {
            var ok = await EnsureDatabaseAsync(); if (!ok) return "{}";
            var snap = await root.Child("users").Child(uid).GetValueAsync();
            return (snap.Exists && !string.IsNullOrEmpty(snap.GetRawJsonValue()))
                ? snap.GetRawJsonValue()
                : "{}";
        }

        public async Task SaveFromJson(string uid, string json)
        {
            var ok = await EnsureDatabaseAsync(); if (!ok) return;
            await root.Child("users").Child(uid).SetRawJsonValueAsync(string.IsNullOrEmpty(json) ? "{}" : json);
        }

        public async Task DeleteByUid(string uid)
        {
            var ok = await EnsureDatabaseAsync(); if (!ok) return;
            await root.Child("users").Child(uid).RemoveValueAsync();
        }

        public PlayerData GetGuest()
        {
            return new PlayerData
            {
                uid = "guest",
                name = PlayerPrefs.GetString(guestNameKey, "Guest"),
                email = "",
                photoUrl = "",
                avatarId = Mathf.Max(0, PlayerPrefs.GetInt(guestAvatarKey, 0)),
                level = Mathf.Max(1, PlayerPrefs.GetInt(guestLevelKey, defaultLevel)),
                coins = Mathf.Max(0, PlayerPrefs.GetInt(guestCoinsKey, defaultCoins)),
                updatedAt = Now()
            };
        }

        public void SaveGuest(PlayerData p)
        {
            if (p == null) return;
            PlayerPrefs.SetString(guestNameKey, string.IsNullOrEmpty(p.name) ? "Guest" : p.name);
            PlayerPrefs.SetInt(guestAvatarKey, Mathf.Max(0, p.avatarId));
            PlayerPrefs.SetInt(guestLevelKey, Mathf.Max(1, p.level));
            PlayerPrefs.SetInt(guestCoinsKey, Mathf.Max(0, p.coins));
            PlayerPrefs.Save();
        }

        public void ClearGuest()
        {
            PlayerPrefs.DeleteKey(guestNameKey);
            PlayerPrefs.DeleteKey(guestAvatarKey);
            PlayerPrefs.DeleteKey(guestLevelKey);
            PlayerPrefs.DeleteKey(guestCoinsKey);
        }

        /// <summary>
        /// Get top N players sorted by coins (highest first)
        /// </summary>
        public async Task<List<PlayerData>> GetTopPlayersByCoins(int limit = 10)
        {
            var ok = await EnsureDatabaseAsync();
            if (!ok) return new List<PlayerData>();

            try
            {
                // Query Firebase - OrderByChild("coins") gets all users sorted by coins
                // LimitToLast gets the highest values
                var query = root.Child("users").OrderByChild("coins").LimitToLast(limit);
                var snap = await query.GetValueAsync();

                var players = new List<PlayerData>();
                
                if (snap.Exists)
                {
                    foreach (var child in snap.Children)
                    {
                        if (!string.IsNullOrEmpty(child.GetRawJsonValue()))
                        {
                            var player = JsonUtility.FromJson<PlayerData>(child.GetRawJsonValue());
                            if (player != null)
                                players.Add(player);
                        }
                    }
                }

                // LimitToLast returns in ascending order, so reverse to get highest first
                players.Reverse();
                return players;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DB] GetTopPlayersByCoins failed: {ex.Message}");
                return new List<PlayerData>();
            }
        }

        /// <summary>
        /// Add test users to Firebase for leaderboard testing
        /// Call this method from Unity Editor or first launch to populate test data
        /// </summary>
        public async Task AddTestUsersForLeaderboard(int count = 15)
        {
            var ok = await EnsureDatabaseAsync();
            if (!ok) return;

            Debug.Log($"[DB] Adding {count} test users to Firebase...");

            string[] testNames = new string[]
            {
                "Aarav", "Vivaan", "Arjun", "Kabir", "Aditya", "Ishaan", "Rohan",
                "Krish", "Anaya", "Diya", "Avni", "Ira", "Myra", "Siya", "Anika", 
                "Kashvi", "Rahul", "Sanjay", "Neha", "Priya", "Ritika", "Manav", 
                "Rakesh", "Pooja", "Ravi", "Sneha", "Amit", "Divya", "Karan", "Meera"
            };

            for (int i = 0; i < count; i++)
            {
                string testUid = $"test_user_{i}_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
                string name = testNames[UnityEngine.Random.Range(0, testNames.Length)];
                int coins = UnityEngine.Random.Range(1000, 15000);
                int level = UnityEngine.Random.Range(1, 50);

                var testUser = new PlayerData
                {
                    uid = testUid,
                    name = name,
                    email = $"{name.ToLower()}{i}@test.com",
                    photoUrl = "",
                    avatarId = UnityEngine.Random.Range(0, 8),
                    level = level,
                    coins = coins,
                    updatedAt = Now()
                };

                string json = JsonUtility.ToJson(testUser);
                await root.Child("users").Child(testUid).SetRawJsonValueAsync(json);
            }

            Debug.Log($"[DB] ? Added {count} test users successfully!");
        }

        /// <summary>
        /// Get user rank by coins (1 = highest coins)
        /// </summary>
        public async Task<int> GetUserRankByCoins(string uid)
        {
            var ok = await EnsureDatabaseAsync();
            if (!ok) return -1;

            try
            {
                // Get current user's coins
                var userSnap = await root.Child("users").Child(uid).GetValueAsync();
                if (!userSnap.Exists) return -1;

                var userData = JsonUtility.FromJson<PlayerData>(userSnap.GetRawJsonValue());
                if (userData == null) return -1;

                int userCoins = userData.coins;

                // Count how many users have more coins
                var query = root.Child("users").OrderByChild("coins").StartAt(userCoins + 1);
                var snap = await query.GetValueAsync();

                int usersWithMoreCoins = 0;
                if (snap.Exists)
                {
                    foreach (var child in snap.Children)
                    {
                        usersWithMoreCoins++;
                    }
                }

                // Rank = number of users with more coins + 1
                return usersWithMoreCoins + 1;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DB] GetUserRankByCoins failed: {ex.Message}");
                return -1;
            }
        }
    }
}
