using Firebase.Auth;
using Google;
using UnityEngine;

namespace CrystalUnbolt
{
    public class CrystalAppGlue : MonoBehaviour
    {
        public CrystalLoginAuthManager auth;
        public CrystalPlayerDataService data;

        private void OnEnable()
        {
            auth.OnSignedIn += HandleSignedIn;
            auth.OnSignedOut += HandleSignedOut;

            if (auth != null && auth.CurrentUser != null)
            {
                HandleSignedIn(auth.CurrentUser, null);
            }
        }


        private void OnDisable()
        {
            auth.OnSignedIn -= HandleSignedIn;
            auth.OnSignedOut -= HandleSignedOut;
        }

        private async void HandleSignedIn(FirebaseUser fUser, GoogleSignInUser gUser)
        {
            string fbName = gUser?.DisplayName ?? "Player";
            string fbEmail = gUser?.Email ?? "";
            string fbPhoto = (gUser?.ImageUrl != null) ? gUser.ImageUrl.ToString() : "";

            var profile = await data.EnsureProfile(fUser, fbName, fbEmail, fbPhoto);

            Debug.Log("[Glue] Profile JSON:\n" + JsonUtility.ToJson(profile, true));
        }


        private void HandleSignedOut()
        {
            Debug.Log("[Glue] Signed out");
        }
    }
}
