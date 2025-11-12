using UnityEngine;
using UnityEngine.UI;

namespace CrystalUnbolt
{
    /// <summary>
    /// Helper script to easily set up Apple login button in Unity Inspector
    /// </summary>
    public class CrystalAppleLoginSetupHelper : MonoBehaviour
    {
        [Header("Setup Instructions")]
        [TextArea(5, 10)]
        public string setupInstructions = 
            "1. Assign this script to your Apple Login Button GameObject\n" +
            "2. Assign the CrystalLoginAuthManager reference\n" +
            "3. Assign the CrystalAppleLoginButton script to the same GameObject\n" +
            "4. The button will automatically show only on iOS devices\n" +
            "5. Google login will show on Android and iOS devices";

        [Header("References")]
        [SerializeField] private CrystalLoginAuthManager CrystalLoginAuthManager;
        [SerializeField] private CrystalAppleLoginButton CrystalAppleLoginButton;

        private void Awake()
        {
            // Auto-setup if references are missing
            if (CrystalLoginAuthManager == null)
            {
                CrystalLoginAuthManager = FindObjectOfType<CrystalLoginAuthManager>();
                if (CrystalLoginAuthManager == null)
                {
                    Debug.LogError("[AppleLoginSetupHelper] CrystalLoginAuthManager not found! Please assign it manually.");
                }
            }

            if (CrystalAppleLoginButton == null)
            {
                CrystalAppleLoginButton = GetComponent<CrystalAppleLoginButton>();
                if (CrystalAppleLoginButton == null)
                {
                    Debug.LogError("[AppleLoginSetupHelper] CrystalAppleLoginButton script not found! Please add it to this GameObject.");
                }
            }
        }

        private void Start()
        {
            // Ensure the button is properly configured
            if (CrystalAppleLoginButton != null && CrystalLoginAuthManager != null)
            {
                // The CrystalAppleLoginButton will handle the rest
                Debug.Log("[AppleLoginSetupHelper] Apple login button setup complete!");
            }
        }

        [ContextMenu("Auto Setup Apple Login")]
        private void AutoSetupAppleLogin()
        {
            if (CrystalLoginAuthManager == null)
            {
                CrystalLoginAuthManager = FindObjectOfType<CrystalLoginAuthManager>();
            }

            if (CrystalAppleLoginButton == null)
            {
                CrystalAppleLoginButton = GetComponent<CrystalAppleLoginButton>();
                if (CrystalAppleLoginButton == null)
                {
                    CrystalAppleLoginButton = gameObject.AddComponent<CrystalAppleLoginButton>();
                }
            }

            Debug.Log("[AppleLoginSetupHelper] Auto setup completed!");
        }
    }
}
