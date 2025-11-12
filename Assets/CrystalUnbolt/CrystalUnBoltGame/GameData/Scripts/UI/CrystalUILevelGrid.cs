using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using CrystalUnbolt.Map;
using DG.Tweening;

namespace CrystalUnbolt
{
    public class CrystalUILevelGrid : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private GameObject levelButtonPrefab; // Use Map Level prefab here
        public Canvas canvas;
        [SerializeField] private ScrollRect scrollRect; // For auto-scrolling to current level
        [SerializeField] private Transform gridContainer; // Assign the "Content" GameObject
        [SerializeField] private int columns = 5; // Number of columns in grid
        [SerializeField] private float spacing = 20f;

        [Header("Animation Settings")]
        [SerializeField] private CanvasGroup canvasGroup; // For fade animation
        [SerializeField] private RectTransform panelRectTransform; // For scale animation
        [SerializeField] private float animationDuration = 0.3f; // Faster panel animation
        [SerializeField] private float buttonStaggerDelay = 0.02f; // Delay between each button appearing

        private List<CrystalMapLevelBehavior> spawnedButtons = new List<CrystalMapLevelBehavior>();
        private bool isInitialized = false;

        private void OnEnable()
        {
            Debug.Log("[LevelGrid] OnEnable called!");
            
            // Ensure canvas is enabled and visible
            if (canvas != null)
            {
                canvas.enabled = true;
                Debug.Log("[LevelGrid] Canvas enabled");
            }
            
            // Ensure CanvasGroup is visible
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                Debug.Log("[LevelGrid] CanvasGroup alpha set to 1");
            }
            
            // Ensure scale is normal (not from animation)
            if (panelRectTransform != null)
                panelRectTransform.localScale = Vector3.one;
            
            // Auto-generate levels when GridPanel becomes active
            if (!isInitialized)
            {
                Debug.Log("[LevelGrid] GridPanel enabled, attempting to generate levels...");
                
                // Check if database is ready
               /* if (CrystalLevelController.Database != null)
                {
                    GenerateLevelButtons();
                    isInitialized = true;
                }
                else
                {
                    Debug.LogWarning("[LevelGrid] Database not ready yet, will retry when database loads...");
                    StartCoroutine(WaitForDatabaseAndGenerate());
                }*/
            }
        }

        private System.Collections.IEnumerator WaitForDatabaseAndGenerate()
        {
            // Wait until database is initialized
            int maxAttempts = 100; // 10 seconds max wait
            int attempts = 0;
            
            while (CrystalLevelController.Database == null && attempts < maxAttempts)
            {
                yield return new WaitForSeconds(0.1f);
                attempts++;
            }

            if (CrystalLevelController.Database != null)
            {
                Debug.Log("[LevelGrid] Database ready! Generating levels now...");
                GenerateLevelButtons();
                isInitialized = true;
            }
            else
            {
                Debug.LogError("[LevelGrid] Failed to load database after 10 seconds!");
            }
        }

        public void ShowLevelGrid()
        {
            Debug.Log("[LevelGrid] ShowLevelGrid called!");
            
            // Enable canvas
            if (canvas != null)
                canvas.enabled = true;
            
            // Generate level buttons if not already done
            if (!isInitialized && CrystalLevelController.Database != null)
            {
                GenerateLevelButtons();
                isInitialized = true;
            }
            else if (!isInitialized)
            {
                Debug.LogWarning("[LevelGrid] Database not ready in ShowLevelGrid, starting retry...");
                StartCoroutine(WaitForDatabaseAndGenerate());
            }
            
            // Play opening animation
            PlayShowAnimation();
        }

        public void RefreshLevels()
        {
            // Refresh levels to update lock/unlock states
            Debug.Log("[LevelGrid] Refreshing levels...");
            GenerateLevelButtons();
        }

        public void ShowGridOnMainMenu()
        {
            Debug.Log("[LevelGrid] ShowGridOnMainMenu called!");
            
            // Force enable canvas
            if (canvas != null)
            {
                canvas.enabled = true;
            }
            
            // Force set alpha to 1
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
            
            // Force scale to 1
            if (panelRectTransform != null)
            {
                panelRectTransform.localScale = Vector3.one;
            }
            
            // Make sure GameObject is active
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
            
            Debug.Log($"[LevelGrid] Canvas enabled: {canvas?.enabled}, Alpha: {canvasGroup?.alpha}, GameObject active: {gameObject.activeSelf}");
        }

        private void PlayShowAnimation()
        {
            // Setup initial states
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, animationDuration).SetEasing(Ease.Type.CubicOut);
            }

            if (panelRectTransform != null)
            {
                panelRectTransform.localScale = Vector3.zero;
                panelRectTransform.DOScale(Vector3.one, animationDuration).SetEasing(Ease.Type.BackOut);
            }
            else
            {
                // Fallback: animate the grid container if panel not assigned
                if (gridContainer != null)
                {
                    gridContainer.localScale = Vector3.zero;
                    gridContainer.DOScale(Vector3.one, animationDuration).SetEasing(Ease.Type.BackOut);
                }
            }

            // Animate each level button appearing with stagger effect
            AnimateLevelButtonsAppearance();
        }

        private void Start()
        {
            // Setup grid layout
            if (gridContainer != null)
            {
                GridLayoutGroup gridLayout = gridContainer.GetComponent<GridLayoutGroup>();
                if (gridLayout == null)
                    gridLayout = gridContainer.gameObject.AddComponent<GridLayoutGroup>();

                gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayout.constraintCount = columns;
                gridLayout.spacing = new Vector2(spacing, spacing);
                gridLayout.childAlignment = TextAnchor.MiddleCenter;
            }
        }
        public void HideLevelGrid()
        {
            PlayHideAnimation();
        }

        private void OnDisable()
        {
            // Reset initialization flag when GridPanel is disabled
            // This ensures levels regenerate when returning to main menu
            isInitialized = false;
            
            // Unsubscribe from events
            CrystalMapLevelAbstractBehavior.OnLevelClicked -= OnLevelFromGridClicked;
        }

        private void PlayHideAnimation()
        {
            // Fade out
            if (canvasGroup != null)
            {
                canvasGroup.DOFade(0f, animationDuration * 0.7f).SetEasing(Ease.Type.CubicIn);
            }

            // Scale down
            if (panelRectTransform != null)
            {
                panelRectTransform.DOScale(Vector3.zero, animationDuration * 0.7f).SetEasing(Ease.Type.BackIn)
                    .OnComplete(() =>
                    {
                        canvas.enabled = false;
                        ClearButtons();
                    });
            }
            else
            {
                // Fallback
                if (gridContainer != null)
                {
                    gridContainer.DOScale(Vector3.zero, animationDuration * 0.7f).SetEasing(Ease.Type.BackIn)
                        .OnComplete(() =>
                        {
                            canvas.enabled = false;
                            ClearButtons();
                        });
                }
                else
                {
                    // No animation - just hide
                    canvas.enabled = false;
                    ClearButtons();
                }
            }
        }

        private void AnimateLevelButtonsAppearance()
        {
            // Animate each level button appearing with a staggered effect
            for (int i = 0; i < spawnedButtons.Count; i++)
            {
                if (spawnedButtons[i] != null)
                {
                    CrystalMapLevelBehavior mapLevel = spawnedButtons[i];
                    Transform btnTransform = mapLevel.transform;
                    
                    // Disable jelly animation temporarily to prevent conflicts
                    CrystalJellyLevelAnimation jellyAnim = mapLevel.GetComponent<CrystalJellyLevelAnimation>();
                    if (jellyAnim != null)
                    {
                        jellyAnim.StopAnimation();
                    }
                    
                    btnTransform.localScale = Vector3.zero;
                    
                    // Stagger delay based on index
                    float delay = i * buttonStaggerDelay;
                    
                    int capturedIndex = i;
                    btnTransform.DOScale(Vector3.one, 0.25f) // Faster button pop-in
                        .SetDelay(delay)
                        .SetEasing(Ease.Type.BackOut)
                        .OnComplete(() =>
                        {
                            // After appearance animation completes, start jelly animation if it's the current level
                            if (capturedIndex == CrystalLevelController.MaxReachedLevelIndex && jellyAnim != null)
                            {
                                jellyAnim.StartAnimation();
                            }
                        });
                }
            }
        }

        private void GenerateLevelButtons()
        {
            ClearButtons();

            // Safety check: Ensure database is available
            if (CrystalLevelController.Database == null)
            {
                Debug.LogError("[LevelGrid] Database is NULL! Cannot generate levels.");
                return;
            }

            // Get total levels from database and generate ALL of them
            int totalLevels = CrystalLevelController.Database.AmountOfLevels;
            Debug.Log($"[LevelGrid] Generating {totalLevels} levels starting from level 1...");
            
            for (int i = 0; i < totalLevels; i++)
            {
                CreateLevelButton(i);
            }
            
            Debug.Log($"[LevelGrid] Generated {spawnedButtons.Count} level buttons successfully!");
            
            // Scroll to show current level after generation
            StartCoroutine(ScrollToCurrentLevelDelayed());
        }

        private System.Collections.IEnumerator ScrollToCurrentLevelDelayed()
        {
            // Wait a frame for layout to update
            yield return null;
            yield return null; // Wait 2 frames to ensure Content Size Fitter updates

            ScrollToCurrentLevel();
        }

        private void ScrollToCurrentLevel()
        {
            if (scrollRect == null || spawnedButtons.Count == 0)
                return;

            // Option 1: Scroll to current level (default)
            int currentLevelIndex = CrystalLevelController.MaxReachedLevelIndex;
            
            // Option 2: Always scroll to top (Level 1)
            // Uncomment the line below to always show Level 1 first:
            // currentLevelIndex = 0;
            
            // Clamp to valid range
            if (currentLevelIndex >= spawnedButtons.Count)
                currentLevelIndex = spawnedButtons.Count - 1;

            // Calculate scroll position to show selected level
            RectTransform content = gridContainer as RectTransform;
            if (content == null || spawnedButtons[currentLevelIndex] == null)
                return;

            RectTransform currentLevelRect = spawnedButtons[currentLevelIndex].GetComponent<RectTransform>();
            
            // Calculate normalized position (0 = top, 1 = bottom)
            float buttonPos = Mathf.Abs(currentLevelRect.anchoredPosition.y);
            float contentHeight = content.rect.height;
            float viewportHeight = scrollRect.viewport.rect.height;
            
            // Scroll to show selected level in the viewport
            float normalizedPos = Mathf.Clamp01((buttonPos - viewportHeight * 0.3f) / (contentHeight - viewportHeight));
            
            scrollRect.verticalNormalizedPosition = 1f - normalizedPos;
            
            Debug.Log($"[LevelGrid] Scrolled to level: {currentLevelIndex + 1}");
        }

        private void CreateLevelButton(int levelIndex)
        {
            Debug.Log("Create levels ");
            if (levelButtonPrefab == null)
            {
                Debug.LogError("[LevelGrid] Level button prefab is NULL! Please assign Map Level prefab in Inspector.");
                return;
            }

            if (gridContainer == null)
            {
                Debug.LogError("[LevelGrid] Grid container is NULL! Please assign Content GameObject in Inspector.");
                return;
            }

            // Instantiate the Map Level prefab
            GameObject buttonObj = Instantiate(levelButtonPrefab, gridContainer);
            Debug.Log($"[LevelGrid] Instantiated level {levelIndex + 1} button: {buttonObj.name}");
            
            CrystalMapLevelBehavior mapLevel = buttonObj.GetComponent<CrystalMapLevelBehavior>();

            if (mapLevel == null)
            {
                Debug.LogError("[LevelGrid] Map Level prefab doesn't have CrystalMapLevelBehavior component!");
                Destroy(buttonObj);
                return;
            }

            spawnedButtons.Add(mapLevel);

            // Use the existing Init method from CrystalMapLevelBehavior
            // This automatically sets up the level number, colors, and states
            mapLevel.Init(levelIndex);

            // Subscribe to level click event (only once, will be handled in OnLevelFromGridClicked)
            if (spawnedButtons.Count == 1)
            {
                CrystalMapLevelAbstractBehavior.OnLevelClicked += OnLevelFromGridClicked;
            }
        }

        private void OnLevelFromGridClicked(int levelIndex)
        {
            // Unsubscribe to prevent multiple calls
            CrystalMapLevelAbstractBehavior.OnLevelClicked -= OnLevelFromGridClicked;

            OnLevelButtonClicked(levelIndex);
        }

        private void OnLevelButtonClicked(int levelIndex)
        {
            SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif

            Debug.Log($"[LevelGrid] Level {levelIndex + 1} clicked!");

            // Hide grid
            HideLevelGrid();

            // Load level (reuse existing logic from main menu)
            if (CrystalLivesSystem.Lives > 0 || CrystalLivesSystem.InfiniteMode)
            {
                ScreenOverlay.Show(0.3f, () =>
                {
                    ScreenManager.DisableScreen<CrystalUIMainMenu>();
                    CrystalGameManager.LoadLevel(levelIndex);
                    ScreenOverlay.Hide(0.3f);
                }, true);
            }
            else
            {
                CrystalUIAddLivesPanel.Show((bool lifeAdded) =>
                {
                    if (lifeAdded)
                    {
                        ScreenOverlay.Show(0.3f, () =>
                        {
                            ScreenManager.DisableScreen<CrystalUIMainMenu>();
                            CrystalGameManager.LoadLevel(levelIndex);
                            ScreenOverlay.Hide(0.3f);
                        }, true);
                    }
                });
            }
        }

        private void ClearButtons()
        {
            // Unsubscribe from level click events
            CrystalMapLevelAbstractBehavior.OnLevelClicked -= OnLevelFromGridClicked;

            foreach (CrystalMapLevelBehavior btn in spawnedButtons)
            {
                if (btn != null)
                    Destroy(btn.gameObject);
            }
            spawnedButtons.Clear();
        }
    }
}

