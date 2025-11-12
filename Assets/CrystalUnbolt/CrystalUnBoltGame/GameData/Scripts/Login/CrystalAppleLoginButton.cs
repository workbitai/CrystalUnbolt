using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CrystalUnbolt
{
    public class CrystalAppleLoginButton : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CrystalLoginAuthManager CrystalLoginAuthManager;
        
        [Header("UI Elements")]
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI  buttonText;
        
        [Header("Button Text")]
        [SerializeField] private string signInText = "Sign in with Apple";
        [SerializeField] private string signingInText = "Signing in...";
        
        private void Awake()
        {
            if (button == null)
                button = GetComponent<Button>();
                
            if (buttonText == null)
                buttonText = GetComponentInChildren<TextMeshProUGUI>();
                
            // Auto-find CrystalLoginAuthManager if not assigned
            if (CrystalLoginAuthManager == null)
            {
                CrystalLoginAuthManager = FindObjectOfType<CrystalLoginAuthManager>();
                if (CrystalLoginAuthManager != null)
                {
                    Debug.Log("[AppleLoginButton] Auto-found CrystalLoginAuthManager");
                }
            }
        }
        
        private void Start()
        {
            if (button != null)
            {
                button.onClick.AddListener(OnAppleLoginClicked);
            }
            
            if (CrystalLoginAuthManager == null)
            {
                Debug.LogError("[AppleLoginButton] CrystalLoginAuthManager not assigned and could not be found!");
            }
        }
        
        public void OnAppleLoginClicked()
        {
            if (CrystalLoginAuthManager == null)
            {
                Debug.LogError("[AppleLoginButton] CrystalLoginAuthManager is null!");
                return;
            }
            
            StartCoroutine(HandleAppleLogin());
        }
        
        private IEnumerator HandleAppleLogin()
        {
            if (buttonText != null)
            {
                buttonText.text = signingInText;
            }
            
            if (button != null)
            {
                button.interactable = false;
            }
            
            var task = CrystalLoginAuthManager.SignInWithApple();
            
            yield return new WaitUntil(() => task.IsCompleted);
            
            if (buttonText != null)
            {
                buttonText.text = signInText;
            }
            
            if (button != null)
            {
                button.interactable = true;
            }
            
            if (task.Exception != null)
            {
                Debug.LogError("[AppleLoginButton] Apple login failed: " + task.Exception);
            }
            else
            {
                Debug.Log("[AppleLoginButton] Apple login completed successfully");
            }
        }
        
        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnAppleLoginClicked);
            }
        }
    }
}