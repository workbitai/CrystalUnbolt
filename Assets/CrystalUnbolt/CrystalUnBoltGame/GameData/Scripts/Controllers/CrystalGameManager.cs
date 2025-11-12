using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using CrystalUnbolt.Map;
using CrystalUnbolt.SkinStore;

namespace CrystalUnbolt
{
    public class CrystalGameManager : MonoBehaviour
    {
        private static CrystalGameManager instance;

        [DrawReference]
        [SerializeField] CrystalGameData data;

        [LineSpacer]
        [SerializeField] ScreenManager uiController;
        [SerializeField] MusicPlayer musicSource;

        private CrystalLevelController levelController;
        private CrystalParticlesController particlesController;
        private CrystalFloatingTextController floatingTextController;
        private CrystalPUController powerUpController;
        private CrystalMapBehavior mapBehavior;
        private CrystalTutorialController tutorialController;
        private CrystalSkinController skinController;
        private CrystalSkinStoreController skinStoreController;

        public static CrystalGameData Data => instance.data;

        private static bool isGameActive;
        public static bool IsGameActive => isGameActive;

        private void Awake()
        {
            // TEMPORARY: Enable logging to debug UI issue
            // Debug.unityLogger.logEnabled = false;
            instance = this;

            gameObject.CacheComponent(out particlesController);
            gameObject.CacheComponent(out floatingTextController);
            gameObject.CacheComponent(out levelController);
            gameObject.CacheComponent(out powerUpController);
            gameObject.CacheComponent(out mapBehavior);
            gameObject.CacheComponent(out tutorialController);
            gameObject.CacheComponent(out skinController);
            gameObject.CacheComponent(out skinStoreController);

            uiController.Init();

            particlesController.Init();
            floatingTextController.Init();
            musicSource.Init();

            skinController.Init();
            skinStoreController.Init(skinController);
            powerUpController.Init();
            levelController.Init();
            tutorialController.Init();

            uiController.InitializeAllScreens();

            musicSource.Activate();
        }

        private void Start()
        {
            Debug.Log("[GameManager] Start() called");
            
            // Initialize YOUR custom ads system
            MyAdsAdapter.Initialize();
            MyAdsAdapter.EnableBanner();

            if (CrystalLevelAutoRun.CheckIfNeedToAutoRunLevel())
            {
                Debug.Log("[GameManager] Auto-running level");
                LoadLevel(CrystalLevelAutoRun.GetLevelIndex());
            }
            else
            {
                Debug.Log("[GameManager] Opening main menu");
                OpenMainMenu();
            }

            GameLoading.MarkAsReadyToHide();
        }

       

        private void OpenMainMenu()
        {
            Debug.Log("[GameManager] OpenMainMenu called");
            mapBehavior.Show();

            Debug.Log("[GameManager] About to show CrystalUIMainMenu");
            ScreenManager.DisplayScreen<CrystalUIMainMenu>();
            Debug.Log("[GameManager] CrystalUIMainMenu show command sent");
        }

        public static void LoadLevel(int index, GameCallback onLevelLoaded = null)
        {
            Debug.Log($"[GameManager] LoadLevel called with index: {index}");
            
            CrystalLivesSystem.LockLife();

            instance.mapBehavior.Hide();

            MyAdsAdapter.EnableBanner();

            Debug.Log("[GameManager] About to show CrystalUIGame");
            ScreenManager.DisplayScreen<CrystalUIGame>();
            Debug.Log("[GameManager] CrystalUIGame show command sent");

            isGameActive = true;
            Debug.Log("index  =>  "+ index);

            if (index == 0 && instance.data.ShowTutorial)
            {
                CrystalITutorial CrystalFirstLevelTutorial = CrystalTutorialController.GetTutorial(CrystalTutorialID.FirstLevel);

                if (CrystalFirstLevelTutorial != null && !CrystalFirstLevelTutorial.IsFinished)
                {
                    Debug.Log("[Tutorial] Starting First Level Tutorial for Level 1");
                    CrystalFirstLevelTutorial.StartTutorial();
                }
                else
                {
                    Debug.Log("[Tutorial] First Level Tutorial already finished or not found");
                }
            }

            instance.levelController.LoadLevel(index, onLevelLoaded);
        }

        public static void OnLevelCompleted()
        {
            if (!isGameActive)
                return;


            Debug.Log("On Level Complate");
            SoundManager.PlaySound(SoundManager.AudioClips.levelComplete);

            CrystalUIGame gameUI = ScreenManager.GetPage<CrystalUIGame>();
            gameUI.PowerUpsUIController.OnLevelFinished();

            if ((CrystalLevelController.DisplayedLevelIndex + 1) % 3 == 0)
            {
                Debug.Log("Showing reward interstitial ad for level " + (CrystalLevelController.DisplayedLevelIndex + 1));
                MyAdsAdapter.ShowInterstitial(null);
            }

            ScreenManager.CloseScreen<CrystalUIGame>(() =>
            {
                ScreenManager.DisplayScreen<CrystalUIComplete>();
            });

            isGameActive = false;
        }

        public static void OnLevelFailed()
        {
            if (!isGameActive)
                return;

            CrystalUIGame gameUI = ScreenManager.GetPage<CrystalUIGame>();
            gameUI.PowerUpsUIController.OnLevelFinished();

            ScreenManager.CloseScreen<CrystalUIGame>(() =>
            {
                ScreenManager.DisplayScreen<CrystalUIGameOver>();
            });

            isGameActive = false;
        }

        public static void LoadNextLevel(GameCallback onLevelLoaded = null)
        {
            Debug.Log("DisplayedLevelIndex  =>  " + CrystalLevelController.DisplayedLevelIndex);
            LoadLevel(CrystalLevelController.DisplayedLevelIndex, onLevelLoaded);
        }

        public static void ReplayLevel()
        {
            isGameActive = false;

            ScreenOverlay.Show(0.3f, () =>
            {
                CrystalUIGame gameUI = ScreenManager.GetPage<CrystalUIGame>();
                gameUI.PowerUpsUIController.OnLevelFinished();

                LoadLevel(CrystalLevelController.DisplayedLevelIndex);

                ScreenOverlay.Hide(0.3f);
            }, true);
        }

        public static void ReplayStage()
        {
            isGameActive = true;

            CrystalUIGame gameUI = ScreenManager.GetPage<CrystalUIGame>();
            gameUI.PowerUpsUIController.OnLevelFinished();

            instance.levelController.ReloadStage();
        }

        public static void ReturnToMenu()
        {
            isGameActive = false;

            CrystalLevelController.UnloadLevel();

            instance.mapBehavior.Show();
            MyAdsAdapter.HideBanner();

            ScreenManager.DisplayScreenReturn<CrystalUIMainMenu>();

        }

        public static void Revive()
        {
            isGameActive = true;
            CrystalLevelController.IsRaycastEnabled = true;
        }
    }
}


public static class PopupHelper
{
  
    public static void ShowPopup(Transform popup, float duration = 0.6f, float overshoot = 1.2f)
    {
        popup.localScale = Vector3.zero;

      
        Sequence seq = DOTween.Sequence();
        seq.Append(popup.DOScale(Vector3.one * overshoot, duration * 0.4f).SetEase(Ease.OutBack));
        seq.Append(popup.DOScale(Vector3.one, duration * 0.3f).SetEase(Ease.OutSine));
    }

    
    public static void HidePopup(Transform popup, float duration = 0.35f)
    {
        popup.DOScale(Vector3.zero, duration).SetEase(Ease.InBack);
    }
}



namespace CrystalUnbolt.Utility
{
    [DefaultExecutionOrder(-10000)]
    public sealed class ProductionLogMute : MonoBehaviour
    {
        [Header("Editor Play Mode")]
        [Tooltip("If ON, mutes logs while playing in the Unity Editor too.")]
        [SerializeField] private bool muteInEditorPlayMode = false; // CHANGED to false for debugging

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ApplyForBuilds()
        {
#if !UNITY_EDITOR
            ApplyMute(); // Always mute in production builds
#endif
        }

        private void Awake()
        {
#if UNITY_EDITOR
            if (!muteInEditorPlayMode) return;
            ApplyMute();
            DontDestroyOnLoad(gameObject);
#endif
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR
            RestoreDefaults();
#endif
        }

        private static bool _applied;

        private static void ApplyMute()
        {
            if (_applied) return;
            _applied = true;

           Debug.unityLogger.logEnabled = false;

            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
            Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.None);
            Application.SetStackTraceLogType(LogType.Assert, StackTraceLogType.None);
            Application.SetStackTraceLogType(LogType.Exception, StackTraceLogType.None);

            Application.logMessageReceivedThreaded += Swallow;
        }

        private static void RestoreDefaults()
        {
            _applied = false;

            Debug.unityLogger.logEnabled = true;

            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.ScriptOnly);
            Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.ScriptOnly);
            Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.ScriptOnly);
            Application.SetStackTraceLogType(LogType.Assert, StackTraceLogType.ScriptOnly);
            Application.SetStackTraceLogType(LogType.Exception, StackTraceLogType.Full);

            Application.logMessageReceivedThreaded -= Swallow;
        }

        private static void Swallow(string condition, string stackTrace, LogType type) { }
    }
}
