using Google.Impl;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CrystalUnbolt
{
    public class CrystalLoadingGraphics : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI loadingText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private CanvasScaler canvasScaler;
        [SerializeField] private Camera loadingCamera;
        [SerializeField] private Image fillBar; // Assign in inspector

        private float displayedProgress = 0f;  // Smooth visible progress
        private float targetProgress = 0f;     // Actual progress from GameLoading
        private bool isFadingOut = false;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            canvasScaler.MatchSize();

            fillBar.fillAmount = 0f;
            loadingText.text = "Loading... 0%";
        }

        private void OnEnable()
        {
            GameLoading.OnLoading += OnLoading;
            GameLoading.OnLoadingFinished += OnLoadingFinished;
            SceneManager.activeSceneChanged += OnSceneChanged;
        }

        private void OnDisable()
        {
            GameLoading.OnLoading -= OnLoading;
            GameLoading.OnLoadingFinished -= OnLoadingFinished;
            SceneManager.activeSceneChanged -= OnSceneChanged;
        }

        private void OnSceneChanged(Scene oldScene, Scene newScene)
        {
            // When new scene becomes active, immediately hide this loader
            if (this != null)
            {
                gameObject.SetActive(false);
                Destroy(gameObject, 0.1f);
            }
        }


        private void Update()
        {
            // Smoothly move progress towards the actual target
            displayedProgress = Mathf.Lerp(displayedProgress, targetProgress, Time.unscaledDeltaTime * 2f);
            fillBar.fillAmount = displayedProgress;

            int percent = Mathf.RoundToInt(displayedProgress * 100f);
            loadingText.text = $"Loading... {percent}%";
        }

        private void OnLoading(float state, string message)
        {
            // Clamp and store target
            targetProgress = Mathf.Clamp01(state);
        }

        private void OnLoadingFinished()
        {
            if (!isFadingOut)
                StartCoroutine(FadeOutAndDestroy());
        }

        private IEnumerator FadeOutAndDestroy()
        {
            isFadingOut = true;
            // Ensure bar fills fully
            while (displayedProgress < 1f)
            {
                displayedProgress = Mathf.MoveTowards(displayedProgress, 1f, Time.unscaledDeltaTime * 0.8f);
                fillBar.fillAmount = displayedProgress;
                int percent = Mathf.RoundToInt(displayedProgress * 100f);
                loadingText.text = $"Loading... {percent}%";
                yield return null;
            }

            // Short pause before fade out
            yield return new WaitForSecondsRealtime(0.3f);

            float t = 0;
            Color bg = backgroundImage.color;
            Color txt = loadingText.color;
            Color fill = fillBar.color;

            while (t < 0.6f)
            {
                t += Time.unscaledDeltaTime;
                float a = Mathf.Lerp(1f, 0f, t / 0.6f);
                backgroundImage.color = new Color(bg.r, bg.g, bg.b, a);
                loadingText.color = new Color(txt.r, txt.g, txt.b, a);
                fillBar.color = new Color(fill.r, fill.g, fill.b, a);
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
