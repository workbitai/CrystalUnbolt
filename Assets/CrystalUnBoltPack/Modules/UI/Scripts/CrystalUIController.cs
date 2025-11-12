using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalUnbolt
{
    public class ScreenManager : MonoBehaviour
    {
        private static ScreenManager uiController;

        [SerializeField] FloatingCloud currencyCloud;
        [SerializeField] SafeAreaHandler notchSaveArea;
        
        private static List<BaseScreen> pages;
        private static Dictionary<Type, BaseScreen> pagesLink = new Dictionary<Type, BaseScreen>();

        private static List<IPopupWindow> popupWindows;
        public static bool IsPopupOpened => !popupWindows.IsNullOrEmpty();

        private static bool isTablet;
        public static bool IsTablet => isTablet;

        private static Canvas mainCanvas;
        public static Canvas MainCanvas => mainCanvas;
        public static CanvasScaler CanvasScaler { get; private set; }

        private static Camera mainCamera;

        private static GameCallback localPageClosedCallback;

        public static event PageCallback PageOpened;
        public static event PageCallback PageClosed;

        public static event PopupWindowCallback PopupOpened;
        public static event PopupWindowCallback PopupClosed;

        public void Init()
        {
            uiController = this;

            mainCanvas = GetComponent<Canvas>();
            CanvasScaler = GetComponent<CanvasScaler>();

            isTablet = UIUtils.IsWideScreen(Camera.main);
            mainCamera = Camera.main;

            CanvasScaler.matchWidthOrHeight = isTablet ? 1 : 0;

            popupWindows = new List<IPopupWindow>();

            pages = new List<BaseScreen>();
            pagesLink = new Dictionary<Type, BaseScreen>();
            for (int i = 0; i < transform.childCount; i++)
            {
                BaseScreen uiPage = transform.GetChild(i).GetComponent<BaseScreen>();
                if(uiPage != null)
                {
                    uiPage.CacheComponents();

                    if(pagesLink.ContainsKey(uiPage.GetType()))
                    {
                        Debug.LogError($"[UI Controller] Page {uiPage.GetType()} is already added to the ScreenManager. Please remove the duplicate object to resolve this issue.", uiPage);
                        continue;
                    }

                    pagesLink.Add(uiPage.GetType(), uiPage);

                    pages.Add(uiPage);
                }
            }

            // Initialize global overlay
            ScreenOverlay.Init(this);
        }

        public void InitializeAllScreens()
        {
            Debug.Log("[UIController] InitializeAllScreens called - initializing all UI pages");
            
            // Refresh notch save area
            notchSaveArea.Init();

            // Initialize currency cloud
            currencyCloud.Init();

            for (int i = 0; i < pages.Count; i++)
            {
                Debug.Log($"[UIController] Initializing page: {pages[i].GetType().Name}");
                pages[i].Init();
                pages[i].DisableCanvas();
                Debug.Log($"[UIController] Page {pages[i].GetType().Name} canvas disabled");
            }
            
            Debug.Log("[UIController] All pages initialized and disabled");
        }

        public static void ResetPages()
        {
            ScreenManager controller = uiController;
            if (controller != null)
            {
                for (int i = 0; i < pages.Count; i++)
                {
                    if (pages[i].IsPageDisplayed)
                    {
                        pages[i].Unload();
                    }
                }
            }
        }

        public static void DisplayScreen<T>() where T : BaseScreen
        {
            Type pageType = typeof(T);
            Debug.Log($"[UIController] Attempting to display screen: {pageType.Name}");
            
            if (!pagesLink.ContainsKey(pageType))
            {
                Debug.LogError($"[UIController] Page {pageType.Name} not found in pagesLink dictionary!");
                return;
            }
            
            BaseScreen page = pagesLink[pageType];
            if (page == null)
            {
                Debug.LogError($"[UIController] Page {pageType.Name} is null!");
                return;
            }
            
            if (!page.IsPageDisplayed)
            {
                Debug.Log($"[UIController] Showing page {pageType.Name} - PlayShowAnimation");

                page.PlayShowAnimation();
                page.EnableCanvas();
                page.GraphicRaycaster.enabled = true;
            }
            else
            {
                Debug.Log($"[UIController] Page {pageType.Name} is already displayed");
            }
        }
        public static void DisplayScreenReturn<T>() where T : BaseScreen
        {
            Type pageType = typeof(T);
            BaseScreen page = pagesLink[pageType];
            if (!page.IsPageDisplayed)
            {
                Debug.Log("PlayShowAnimation");

                page.PlayShowAnimationMainReturn();
                page.EnableCanvas();
                page.GraphicRaycaster.enabled = true;
            }
        }

        public static void ShowPage(BaseScreen page)
        {
            if (!page.IsPageDisplayed)
            {
                Debug.Log("PlayShowAnimation");
                page.PlayShowAnimation();
                page.EnableCanvas();
                page.GraphicRaycaster.enabled = true;
            }
        }

        public static void CloseScreen<T>(GameCallback onPageClosed = null)
        {
            Type pageType = typeof(T);
            BaseScreen page = pagesLink[pageType];
            if (page.IsPageDisplayed)
            {
                localPageClosedCallback = onPageClosed;

                page.GraphicRaycaster.enabled = false;
                page.PlayHideAnimation();
            }
            else
            {
                onPageClosed?.Invoke();
            }
        }

        public static void DisableScreen<T>()
        {
            Type pageType = typeof(T);
            BaseScreen page = pagesLink[pageType];
            if (page.IsPageDisplayed)
            {
                page.DisableCanvas();

                OnPageClosed(page);
            }
        }

        public static bool IsDisplayed<T>() where T : BaseScreen
        {
            Type type = typeof(T);
            if (pagesLink.ContainsKey(type))
            {
                return pagesLink[type].IsPageDisplayed;
            }

            return false;
        }

        public static void OnPageClosed(BaseScreen page)
        {
            page.DisableCanvas();

            PageClosed?.Invoke(page, page.GetType());

            if (localPageClosedCallback != null)
            {
                localPageClosedCallback.Invoke();
                localPageClosedCallback = null;
            }
        }

        public static void OnPageOpened(BaseScreen page)
        {
            PageOpened?.Invoke(page, page.GetType());
        }

        public static void OnPopupWindowOpened(IPopupWindow popupWindow)
        {
            if(!popupWindows.Contains(popupWindow))
            {
                popupWindows.Add(popupWindow);

                PopupOpened?.Invoke(popupWindow, true);
            }
        }

        public static void OnPopupWindowClosed(IPopupWindow popupWindow)
        {
            if (popupWindows.Contains(popupWindow))
            {
                popupWindows.Remove(popupWindow);

                PopupClosed?.Invoke(popupWindow, false);
            }
        }

        public static T GetPage<T>() where T : BaseScreen
        {
            BaseScreen page;

            if (pagesLink.TryGetValue(typeof(T), out page))
                return (T)page;

            return null;
        }

        public static Vector3 FixUIElementToWorld(Transform target, Vector3 offset)
        {
            Vector3 targPos = target.transform.position + offset;
            Vector3 camForward = mainCamera.transform.forward;

            float distInFrontOfCamera = Vector3.Dot(targPos - (mainCamera.transform.position + camForward), camForward);
            if (distInFrontOfCamera < 0f)
            {
                targPos -= camForward * distInFrontOfCamera;
            }

            return RectTransformUtility.WorldToScreenPoint(mainCamera, targPos);
        }

        private void OnDestroy()
        {
            FloatingCloud.Clear();

            ScreenOverlay.Clear();
        }

        public delegate void PageCallback(BaseScreen page, Type pageType);
        public delegate void PopupWindowCallback(IPopupWindow popupWindow, bool state);
    }
}

// -----------------
// UI Controller v1.2.1
// -----------------

// Changelog
// v 1.2.1
//  Added Editor script that automatically configure CanvasScaler
// v 1.2
//  Added global overlay
// v 1.1
//  Added popup callbacks and methods to handle when a custom window is opened
//  RectTransform can be added to SafeAreaHandler using SafeAreaHandler.RegisterRectTransform method
// v 1.0
//  Basic logic