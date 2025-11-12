using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using CrystalUnbolt.Map;

namespace CrystalUnbolt
{
    public class CrystalUILevelGrid : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private GameObject levelButtonPrefab; // Use Map Level prefab here
        [SerializeField] private Transform gridContainer; // Assign the "Content" GameObject
        [SerializeField] private int columns = 5; // Number of columns in grid
        [SerializeField] private float spacing = 20f;
        [SerializeField] private int levelsPerPage = 25; // 5x5 grid

        [Header("Pagination")]
        [SerializeField] private Button nextPageButton;
        [SerializeField] private Button prevPageButton;
        [SerializeField] private TMP_Text pageNumberText;

        private List<CrystalMapLevelBehavior> spawnedButtons = new List<CrystalMapLevelBehavior>();
        private int currentPage = 0;
        private int totalPages = 0;

        private void Start()
        {
            if (nextPageButton != null)
                nextPageButton.onClick.AddListener(NextPage);

            if (prevPageButton != null)
                prevPageButton.onClick.AddListener(PreviousPage);

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

        public void ShowLevelGrid()
        {
            gameObject.SetActive(true);
            
            // Calculate current page based on max reached level
            int maxLevel = CrystalLevelController.MaxReachedLevelIndex;
            currentPage = maxLevel / levelsPerPage;
            
            GenerateLevelButtons();
        }

        public void HideLevelGrid()
        {
            gameObject.SetActive(false);
            ClearButtons();
        }

        private void GenerateLevelButtons()
        {
            ClearButtons();

            // Get total levels from database
            int totalLevels = CrystalLevelController.Database.AmountOfLevels;
            totalPages = Mathf.CeilToInt((float)totalLevels / levelsPerPage);

            int startLevel = currentPage * levelsPerPage;
            int endLevel = Mathf.Min(startLevel + levelsPerPage, totalLevels);

            for (int i = startLevel; i < endLevel; i++)
            {
                CreateLevelButton(i);
            }

            UpdatePaginationUI();
        }

        private void CreateLevelButton(int levelIndex)
        {
            if (levelButtonPrefab == null || gridContainer == null)
            {
                Debug.LogError("[LevelGrid] Level button prefab or grid container is null!");
                return;
            }

            // Instantiate the Map Level prefab
            GameObject buttonObj = Instantiate(levelButtonPrefab, gridContainer);
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

            // Subscribe to level click event
            CrystalMapLevelAbstractBehavior.OnLevelClicked += OnLevelFromGridClicked;
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

        private void NextPage()
        {
            if (currentPage < totalPages - 1)
            {
                currentPage++;
                GenerateLevelButtons();
                
                SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
                Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif
            }
        }

        private void PreviousPage()
        {
            if (currentPage > 0)
            {
                currentPage--;
                GenerateLevelButtons();
                
                SoundManager.PlaySound(SoundManager.AudioClips.buttonSound);
#if MODULE_HAPTIC
                Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif
            }
        }

        private void UpdatePaginationUI()
        {
            if (pageNumberText != null)
                pageNumberText.text = $"Page {currentPage + 1} / {totalPages}";

            if (prevPageButton != null)
                prevPageButton.interactable = currentPage > 0;

            if (nextPageButton != null)
                nextPageButton.interactable = currentPage < totalPages - 1;
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

        private void OnDestroy()
        {
            if (nextPageButton != null)
                nextPageButton.onClick.RemoveAllListeners();

            if (prevPageButton != null)
                prevPageButton.onClick.RemoveAllListeners();

            // Unsubscribe from events
            CrystalMapLevelAbstractBehavior.OnLevelClicked -= OnLevelFromGridClicked;
        }
    }
}

