using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CrystalUnbolt
{
    [CustomEditor(typeof(ScreenManager))]
    public class UIControllerEditor : Editor
    {
        [InitializeOnLoadMethod]
        public static void CheckCanvasSize()
        {
#if UNITY_6000
            ScreenManager uiController = FindFirstObjectByType<ScreenManager>();
#else
            ScreenManager uiController = FindObjectOfType<ScreenManager>();
#endif

            if (uiController != null)
            {
                CanvasScaler canvasScaler = uiController.gameObject.GetComponent<CanvasScaler>();
                canvasScaler.matchWidthOrHeight = UIUtils.IsWideScreen(Camera.main) ? 1 : 0;
            }
        }

        [MenuItem("CONTEXT/ScreenManager/Create Canvas")]
        public static void CreateNewCanvas(MenuCommand menuCommand)
        {
            ScreenManager uiController = (ScreenManager)menuCommand.context;

            // Create a custom game object
            GameObject canvasObject = new GameObject("UI Custom Canvas");
            canvasObject.transform.SetParent(uiController.transform);
            canvasObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            canvasObject.layer = LayerMask.NameToLayer("UI");

            RectTransform canvasRectTransform = canvasObject.AddComponent<RectTransform>();
            canvasRectTransform.anchorMin = new Vector2(0, 0);
            canvasRectTransform.anchorMax = new Vector2(1.0f, 1.0f);
            canvasRectTransform.sizeDelta = Vector2.zero;

            canvasObject.AddComponent<Canvas>();
            canvasObject.AddComponent<GraphicRaycaster>();

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(canvasObject, "Create " + canvasObject.name);

            Selection.activeObject = canvasObject;
        }
    }
}
