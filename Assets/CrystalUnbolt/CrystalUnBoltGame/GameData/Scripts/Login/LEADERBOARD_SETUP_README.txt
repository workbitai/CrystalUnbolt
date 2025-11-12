================================================================================
                    REAL FIREBASE LEADERBOARD - SETUP GUIDE
================================================================================

આ leaderboard હવે Firebase Realtime Database માંથી real data લાવે છે!

================================================================================
1. FIREBASE DATABASE RULES (IMPORTANT!)
================================================================================

Firebase Console માં જાઓ અને આ rules add કરો:

1. Firebase Console → Realtime Database → Rules
2. આ rules paste કરો:

{
  "rules": {
    "users": {
      ".read": "auth != null",
      ".write": "auth != null",
      ".indexOn": ["coins"],
      "$uid": {
        ".read": "auth != null",
        ".write": "auth != null && auth.uid == $uid"
      }
    }
  }
}

આ rules શું કરે છે:
- ✅ Logged in users એ બધા users read કરી શકે છે (leaderboard માટે)
- ✅ User પોતાનો જ data write કરી શકે છે
- ✅ "coins" field પર index છે (fast queries માટે)

================================================================================
2. TESTING - Test Users Add કરવા
================================================================================

Option A: Inspector માંથી (Recommended)
----------------------------------------
1. Hierarchy માં કોઈ પણ GameObject select કરો
2. Inspector માં "Add Component" → "LeaderboardTestHelper"
3. Right-click on component → "Add Test Users to Firebase"
4. Console માં success message જોશો

Option B: Code માંથી
--------------------
PlayerDataService dataService = FindObjectOfType<PlayerDataService>();
await dataService.AddTestUsersForLeaderboard(15);

Option C: Debug Button (UILeaderBoard માં)
------------------------------------------
1. UILeaderBoard prefab/GameObject select કરો
2. "Debug Add Test Users Button" field માં UI Button assign કરો
3. Play mode માં button click કરો (Editor only)

================================================================================
3. HOW IT WORKS
================================================================================

Auto Sync:
----------
જ્યારે user:
✅ Login કરે → Local coins automatically Firebase માં sync થાય છે
✅ Level complete કરે → Coins Firebase માં update થાય છે
✅ Coins earn કરે → Automatically cloud માં save થાય છે

Leaderboard:
------------
✅ Top 10 users દેખાય છે (highest coins પહેલા)
✅ Ranks automatically calculate થાય છે (1, 2, 3...)
✅ User નું real rank bottom માં દેખાય છે
✅ Real-time data Firebase માંથી

================================================================================
4. TROUBLESHOOTING
================================================================================

Q: Leaderboard ખાલી દેખાય છે?
A: 
   - Firebase Console → Database → Data check કરો કે users છે કે નહીં
   - Test users add કરો (ઉપરની instructions follow કરો)
   - Database Rules check કરો કે ".indexOn": ["coins"] છે કે નહીં

Q: User નો rank દેખાતો નથી?
A:
   - Check કરો કે user login છે કે નહીં
   - CloudProgressSync.PushLocalToCloud() call થાય છે કે નહીં
   - Firebase Console માં user નો coins field update થયો છે કે નહીં

Q: Performance issues?
A:
   - Firebase Console → Database → Rules → Check indexing
   - ".indexOn": ["coins"] add કરો

================================================================================
5. FILES MODIFIED
================================================================================

✅ PlayerDataService.cs
   - GetTopPlayersByCoins() - Top N users fetch કરે છે
   - GetUserRankByCoins() - User rank calculate કરે છે
   - AddTestUsersForLeaderboard() - Test users add કરે છે

✅ UILeaderBoard.cs
   - LoadRealLeaderboardAsync() - Real Firebase data load કરે છે
   - SetMyProfileAsync() - User profile with real rank

✅ LoginAuthManager.cs
   - Login પછી PushLocalToCloud() call કરે છે

✅ AppleAuthManager.cs
   - Apple login પછી PushLocalToCloud() call કરે છે

✅ LeaderboardTestHelper.cs (NEW)
   - Testing માટે helper component

================================================================================
6. NEXT STEPS (OPTIONAL)
================================================================================

□ Leaderboard UI માં refresh button add કરો
□ Pull-to-refresh functionality add કરો
□ Weekly/Monthly leaderboard માટે timestamp based filtering
□ Custom avatar images support (PhotoURL માંથી)
□ Pagination add કરો (10+ users માટે)

================================================================================

