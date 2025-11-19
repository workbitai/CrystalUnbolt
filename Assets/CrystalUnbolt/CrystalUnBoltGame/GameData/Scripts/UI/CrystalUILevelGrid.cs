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
        [SerializeField] private float spacingPercentage = 0.02f; // Spacing as percentage of screen width
        [SerializeField] private float paddingPercentage = 0.04f; // Padding as percentage of screen width
        [SerializeField] private bool useDynamicCellSize = true; // Auto-calculate cell size based on screen
        [SerializeField] private Vector2 fixedCellSize = new Vector2(120f, 120f); // Used if useDynamicCellSize is false

        [Header("Animation Settings")]
        [SerializeField] private CanvasGroup canvasGroup; // For fade animation
        [SerializeField] private RectTransform panelRectTransform; // For scale animation
        [SerializeField] private float animationDuration = 0.3f; // Faster panel animation
        [SerializeField] private float buttonStaggerDelay = 0.02f; // Delay between each button appearing

        [Header("Coming Soon Button")]
        [SerializeField] private GameObject comingSoonButtonPrefab; // Prefab for "Coming Soon" button (similar to level button)
        [SerializeField] private string comingSoonText = "Coming Soon.."; // Text to display on the button
        [SerializeField, Tooltip("Always show the Coming Soon button, ignoring level completion & infinite mode.")]
        private bool alwaysShowComingSoonButton = true;

        private List<CrystalMapLevelBehavior> spawnedButtons = new List<CrystalMapLevelBehavior>();
        private GameObject comingSoonButton; // Reference to the "Coming Soon" button
        private bool isInitialized = false;
        private Vector2 lastScreenSize;

        private void OnEnable()
        {
            Debug.Log("[LevelGrid] OnEnable called!");
            
            // Don't force canvas to be enabled - let it be controlled by ShowLevelGrid()
            // This prevents the grid from showing automatically on startup
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
            
            // Ensure CanvasGroup is visible
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
            
            // Ensure scale is normal
            if (panelRectTransform != null)
            {
                panelRectTransform.localScale = Vector3.one;
            }
            
            // Ensure grid layout is set up for current screen size
            Vector2 currentScreenSize = new Vector2(Screen.width, Screen.height);
            if (currentScreenSize != lastScreenSize)
            {
                Debug.Log($"[LevelGrid] Screen size changed, refreshing layout...");
                SetupGridLayout();
                lastScreenSize = currentScreenSize;
            }
            
            // Generate level buttons if not already done OR if buttons were cleared
            if ((!isInitialized || spawnedButtons.Count == 0) && CrystalLevelController.Database != null)
            {
                Debug.Log($"[LevelGrid] Generating levels in ShowLevelGrid (initialized: {isInitialized}, button count: {spawnedButtons.Count})...");
                GenerateLevelButtons();
                isInitialized = true;
            }
            else if (!isInitialized)
            {
                Debug.LogWarning("[LevelGrid] Database not ready in ShowLevelGrid, starting retry...");
                StartCoroutine(WaitForDatabaseAndGenerate());
            }
            else
            {
                Debug.Log($"[LevelGrid] Levels already generated ({spawnedButtons.Count} buttons), skipping generation");
            }
            
            // Play opening animation
            PlayShowAnimation();
            
            // Check if all levels are complete and add Coming Soon button
            CheckAndAddComingSoonButton();
        }

        public void RefreshLevels()
        {
            // Refresh levels to update lock/unlock states
            Debug.Log("[LevelGrid] Refreshing levels...");
            GenerateLevelButtons();
            
            // Check if all levels are complete and add Coming Soon button
            CheckAndAddComingSoonButton();
        }

        /// <summary>
        /// Public method to manually check and update the coming soon button.
        /// Useful for testing or calling after level completion.
        /// </summary>
        public void UpdateComingSoonButton()
        {
            CheckAndAddComingSoonButton();
        }

        /// <summary>
        /// Force create the Coming Soon button for testing purposes.
        /// </summary>
        [ContextMenu("Force Create Coming Soon Button")]
        public void ForceCreateComingSoonButton()
        {
            Debug.Log("[LevelGrid] Force creating Coming Soon button (manual trigger)");
            if (comingSoonButton != null)
            {
                Destroy(comingSoonButton);
                comingSoonButton = null;
            }
            CreateComingSoonButton();
        }

        public void ShowGridOnMainMenu()
        {
            Debug.Log("[LevelGrid] ShowGridOnMainMenu called!");
            
            // Make sure GameObject is active FIRST
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
                Debug.Log("[LevelGrid] GameObject was inactive, now activated");
            }
            
            // Force enable canvas
            if (canvas != null)
            {
                canvas.enabled = true;
                Debug.Log($"[LevelGrid] Canvas enabled: {canvas.enabled}");
            }
            else
            {
                Debug.LogError("[LevelGrid] Canvas is NULL! Please assign it in Inspector!");
            }
            
            // Force set alpha to 1
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                Debug.Log($"[LevelGrid] CanvasGroup alpha set to: {canvasGroup.alpha}");
            }
            else
            {
                Debug.LogWarning("[LevelGrid] CanvasGroup is NULL! Grid may not be visible. Assign CanvasGroup in Inspector.");
            }
            
            // Force scale to 1
            if (panelRectTransform != null)
            {
                panelRectTransform.localScale = Vector3.one;
                Debug.Log($"[LevelGrid] Panel scale set to: {panelRectTransform.localScale}");
            }
            else
            {
                Debug.LogWarning("[LevelGrid] PanelRectTransform is NULL! Trying to use transform instead.");
                transform.localScale = Vector3.one;
                Debug.Log($"[LevelGrid] Transform scale set to: {transform.localScale}");
            }
            
            // Check if screen size changed and refresh layout if needed
            Vector2 currentScreenSize = new Vector2(Screen.width, Screen.height);
            if (currentScreenSize != lastScreenSize)
            {
                Debug.Log($"[LevelGrid] Screen size changed from {lastScreenSize} to {currentScreenSize}, refreshing layout...");
                SetupGridLayout();
                lastScreenSize = currentScreenSize;
            }
            
            // Always regenerate levels to ensure they're up to date when returning from game
            if (CrystalLevelController.Database != null)
            {
                Debug.Log("[LevelGrid] Generating levels in ShowGridOnMainMenu...");
                GenerateLevelButtons();
                isInitialized = true;
            }
            else
            {
                Debug.LogWarning("[LevelGrid] Database not ready in ShowGridOnMainMenu, starting retry...");
                StartCoroutine(WaitForDatabaseAndGenerate());
            }
            
            // Check if all levels are complete and show message
            CheckAndShowNewLevelsMessage();
            
            Debug.Log($"[LevelGrid] FINAL STATE - Canvas: {canvas?.enabled}, Alpha: {canvasGroup?.alpha}, Scale: {(panelRectTransform != null ? panelRectTransform.localScale : transform.localScale)}, GameObject active: {gameObject.activeSelf}, Levels: {spawnedButtons.Count}");
        }

        private void PlayShowAnimation()
        {
            Debug.Log("[LevelGrid] PlayShowAnimation started");
            
            // Setup initial states
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, animationDuration).SetEasing(Ease.Type.CubicOut)
                    .OnComplete(() => Debug.Log("[LevelGrid] CanvasGroup fade animation complete"));
                Debug.Log("[LevelGrid] CanvasGroup fade animation started");
            }
            else
            {
                Debug.LogWarning("[LevelGrid] CanvasGroup is NULL in PlayShowAnimation!");
            }

            if (panelRectTransform != null)
            {
                panelRectTransform.localScale = Vector3.zero;
                panelRectTransform.DOScale(Vector3.one, animationDuration).SetEasing(Ease.Type.BackOut)
                    .OnComplete(() => Debug.Log("[LevelGrid] Panel scale animation complete"));
                Debug.Log("[LevelGrid] Panel scale animation started");
            }
            else
            {
                Debug.LogWarning("[LevelGrid] PanelRectTransform is NULL in PlayShowAnimation!");
                // Fallback: animate the grid container if panel not assigned
                if (gridContainer != null)
                {
                    gridContainer.localScale = Vector3.zero;
                    gridContainer.DOScale(Vector3.one, animationDuration).SetEasing(Ease.Type.BackOut);
                    Debug.Log("[LevelGrid] Grid container scale animation started (fallback)");
                }
            }

            // Animate each level button appearing with stagger effect
            AnimateLevelButtonsAppearance();
        }

        private void Awake()
        {
            // Auto-find components if not assigned
            if (canvas == null)
            {
                canvas = GetComponent<Canvas>();
                Debug.Log($"[LevelGrid] Canvas auto-found: {canvas != null}");
            }
            
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                Debug.Log($"[LevelGrid] CanvasGroup auto-found: {canvasGroup != null}");
            }
            
            if (panelRectTransform == null)
            {
                panelRectTransform = GetComponent<RectTransform>();
                Debug.Log($"[LevelGrid] RectTransform auto-found: {panelRectTransform != null}");
            }
        }
        
        private void Start()
        {
            lastScreenSize = new Vector2(Screen.width, Screen.height);
            SetupGridLayout();
        }

        private void SetupGridLayout()
        {
            if (gridContainer == null)
                return;

            GridLayoutGroup gridLayout = gridContainer.GetComponent<GridLayoutGroup>();
            if (gridLayout == null)
                gridLayout = gridContainer.gameObject.AddComponent<GridLayoutGroup>();

            // Get viewport/screen dimensions for responsive sizing
            RectTransform viewportRect = scrollRect != null ? scrollRect.viewport : null;
            float availableWidth = viewportRect != null ? viewportRect.rect.width : Screen.width;
            float availableHeight = viewportRect != null ? viewportRect.rect.height : Screen.height;

            // Calculate responsive padding and spacing
            int padding = Mathf.RoundToInt(availableWidth * paddingPercentage);
            float spacing = availableWidth * spacingPercentage;

            // Calculate cell size dynamically to fit screen perfectly
            Vector2 cellSize;
            if (useDynamicCellSize)
            {
                // Calculate cell size based on available width and columns
                float totalSpacing = spacing * (columns - 1);
                float totalPadding = padding * 2;
                float availableForCells = availableWidth - totalSpacing - totalPadding;
                float cellWidth = availableForCells / columns;
                
                // Make cells square
                cellSize = new Vector2(cellWidth, cellWidth);
            }
            else
            {
                cellSize = fixedCellSize;
            }

            // Configure grid layout
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = columns;
            gridLayout.spacing = new Vector2(spacing, spacing);
            gridLayout.cellSize = cellSize;
            gridLayout.childAlignment = TextAnchor.UpperCenter;
            gridLayout.padding = new RectOffset(padding, padding, padding, padding);

            Debug.Log($"[LevelGrid] Grid configured for screen width {availableWidth}px: {columns} columns, {cellSize} cell size, {spacing:F1} spacing, {padding}px padding");
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
            
            // Check if all levels are complete and add "Coming Soon" button
            CheckAndAddComingSoonButton();
            
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

            // Scroll to current level with better centering
            int currentLevelIndex = CrystalLevelController.MaxReachedLevelIndex;
            
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
            
            // Center the current level in viewport (more visually pleasing)
            float normalizedPos = Mathf.Clamp01((buttonPos - viewportHeight * 0.4f) / Mathf.Max(1f, contentHeight - viewportHeight));
            
            scrollRect.verticalNormalizedPosition = 1f - normalizedPos;
            
            Debug.Log($"[LevelGrid] Scrolled to level: {currentLevelIndex + 1}, Position: {normalizedPos:F2}");
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
                    else
                    {
                        // User closed the panel without adding life - ensure grid is visible
                        Debug.Log("[LevelGrid] User closed lives panel - ensuring Grid Panel is visible");
                        
                        // Force canvas and visibility in case something hid it
                        if (canvas != null)
                            canvas.enabled = true;
                        
                        if (canvasGroup != null)
                        {
                            canvasGroup.alpha = 1f;
                            canvasGroup.interactable = true;
                            canvasGroup.blocksRaycasts = true;
                        }
                        
                        if (panelRectTransform != null)
                            panelRectTransform.localScale = Vector3.one;
                        
                        // Refresh levels to show updated states
                        RefreshLevels();
                        
                        Debug.Log("[LevelGrid] Grid Panel visibility forced after lives panel closed");
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
            
            // Clear coming soon button
            if (comingSoonButton != null)
            {
                Destroy(comingSoonButton);
                comingSoonButton = null;
            }
        }

        private void CheckAndAddComingSoonButton()
        {
            if (CrystalLevelController.Database == null)
            {
                Debug.LogWarning("[LevelGrid] Database is null, cannot check level completion");
                return;
            }

            int totalLevels = CrystalLevelController.Database.AmountOfLevels;
            int displayedLevel = CrystalLevelController.DisplayedLevelIndex;
            int maxReachedLevel = CrystalLevelController.MaxReachedLevelIndex;
            
            // Check if all levels are complete
            // Levels are 0-indexed, so last level index is (totalLevels - 1)
            // When last level (index totalLevels-1) is completed:
            // - DisplayLevelIndex becomes totalLevels (incremented after completion)
            // - MaxReachedLevelIndex becomes totalLevels but gets clamped to totalLevels - 1
            // So we check both conditions:
            bool allLevelsComplete = displayedLevel >= totalLevels || 
                                     maxReachedLevel >= totalLevels - 1;

            bool infiniteLevels = CrystalGameManager.Data != null && CrystalGameManager.Data.InfiniteLevels;
            bool shouldShow = alwaysShowComingSoonButton || (!infiniteLevels && allLevelsComplete);

            Debug.Log($"[LevelGrid] Checking for Coming Soon button: MaxReached={maxReachedLevel}, Displayed={displayedLevel}, Total={totalLevels}, AllComplete={allLevelsComplete}, Infinite={infiniteLevels}, AlwaysShow={alwaysShowComingSoonButton}, ShouldShow={shouldShow}");

            if (shouldShow)
            {
                Debug.Log("[LevelGrid] All conditions met! Creating Coming Soon button...");
                CreateComingSoonButton();
            }
            else
            {
                RemoveComingSoonButton();
            }
        }

        private void CreateComingSoonButton()
        {
            // If already exists, don't create again
            if (comingSoonButton != null)
            {
                Debug.Log("[LevelGrid] Coming Soon button already exists");
                return;
            }

            if (gridContainer == null)
            {
                Debug.LogError("[LevelGrid] Grid container is NULL! Cannot create Coming Soon button.");
                return;
            }

            GameObject prefabToUse = comingSoonButtonPrefab != null ? comingSoonButtonPrefab : levelButtonPrefab;
            
            if (prefabToUse == null)
            {
                Debug.LogError("[LevelGrid] Both comingSoonButtonPrefab and levelButtonPrefab are null! Cannot create Coming Soon button. Please assign at least one prefab in Inspector.");
                return;
            }

            Debug.Log($"[LevelGrid] Instantiating Coming Soon button from prefab: {prefabToUse.name}");
            comingSoonButton = Instantiate(prefabToUse, gridContainer);
            comingSoonButton.name = "ComingSoonButton"; // Rename for clarity
            
            Debug.Log("[LevelGrid] Coming Soon button instantiated successfully");

            // Setup the button
            SetupComingSoonButton(comingSoonButton);
            
            // Animate the button appearance
            AnimateComingSoonButton();
            
            Debug.Log("[LevelGrid] Coming Soon button setup complete!");
        }

        private void SetupComingSoonButton(GameObject buttonObj)
        {
            if (buttonObj == null)
            {
                Debug.LogError("[LevelGrid] buttonObj is null in SetupComingSoonButton!");
                return;
            }

            Debug.Log("[LevelGrid] Setting up Coming Soon button...");

            // Get or add components
            Button button = buttonObj.GetComponent<Button>();
            if (button == null)
            {
                button = buttonObj.AddComponent<Button>();
                Debug.Log("[LevelGrid] Added Button component to Coming Soon button");
            }

            // Find text component and update it
            TextMeshProUGUI textComponent = buttonObj.GetComponentInChildren<TextMeshProUGUI>(true); // Include inactive
            if (textComponent == null)
            {
                // Try to find by common names
                Transform[] allChildren = buttonObj.GetComponentsInChildren<Transform>(true);
                foreach (Transform child in allChildren)
                {
                    TextMeshProUGUI tmp = child.GetComponent<TextMeshProUGUI>();
                    if (tmp != null)
                    {
                        textComponent = tmp;
                        break;
                    }
                }
            }

            if (textComponent != null)
            {
                textComponent.text = comingSoonText;
                Debug.Log($"[LevelGrid] Updated Coming Soon button text to: {comingSoonText}");
            }
            else
            {
                Debug.LogWarning("[LevelGrid] Could not find TextMeshProUGUI component in Coming Soon button! Text may not be updated.");
            }

            // Make button clickable but show message on click
            button.interactable = true;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
                Haptic.Play(Haptic.HAPTIC_HARD);
#endif
                Debug.Log("[LevelGrid] Coming Soon button clicked - New levels are coming soon!");
            });

            // Disable level behavior if it exists to prevent level loading
            CrystalMapLevelBehavior levelBehavior = buttonObj.GetComponent<CrystalMapLevelBehavior>();
            if (levelBehavior != null)
            {
                levelBehavior.enabled = false;
                Debug.Log("[LevelGrid] Disabled CrystalMapLevelBehavior on Coming Soon button");
            }

            // Disable any level click handlers
            CrystalMapLevelAbstractBehavior abstractBehavior = buttonObj.GetComponent<CrystalMapLevelAbstractBehavior>();
            if (abstractBehavior != null)
            {
                abstractBehavior.enabled = false;
                Debug.Log("[LevelGrid] Disabled CrystalMapLevelAbstractBehavior on Coming Soon button");
            }

            Debug.Log("[LevelGrid] Coming Soon button setup complete!");
        }

        private void AnimateComingSoonButton()
        {
            if (comingSoonButton == null) return;

            RectTransform rect = comingSoonButton.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.localScale = Vector3.zero;
                
                // Calculate delay based on button index (after all level buttons)
                float delay = spawnedButtons.Count * buttonStaggerDelay;
                
                rect.DOScale(Vector3.one, 0.25f)
                    .SetDelay(delay)
                    .SetEasing(Ease.Type.BackOut)
                    .OnComplete(() =>
                    {
                        // Add gentle pulse animation
                        rect.transform.DOPingPongScale(1.0f, 1.05f, 0.9f,
                            Ease.Type.QuadIn, Ease.Type.QuadOut, unscaledTime: true);
                    });
            }
        }

        private void RemoveComingSoonButton()
        {
            if (comingSoonButton != null)
            {
                Destroy(comingSoonButton);
                comingSoonButton = null;
                Debug.Log("[LevelGrid] Removed 'Coming Soon' button");
            }
        }

        // Keep old methods for backward compatibility (but they won't be used)
        private void CheckAndShowNewLevelsMessage()
        {
            // This method is now replaced by CheckAndAddComingSoonButton
            // But keeping it for backward compatibility
            CheckAndAddComingSoonButton();
        }
    }
}

