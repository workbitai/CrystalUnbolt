using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CrystalUnbolt
{
    [InitializeOnLoad]
    public static class AutoInitializerLoader
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void LoadMain()
        {
            if (!CoreEditor.AutoLoadInitializer) return;

            Scene currentScene = SceneManager.GetActiveScene();
            if (currentScene != null)
            {
                if (currentScene.name != CoreEditor.InitSceneName)
                {
#if UNITY_6000
                    CrystalInitializer initializer = Object.FindFirstObjectByType<CrystalInitializer>();
#else
                    CrystalInitializer initializer = Object.FindObjectOfType<CrystalInitializer>();
#endif

                    if (initializer == null)
                    {
                        GameObject initializerPrefab = EditorUtils.GetAsset<GameObject>("CrystalInitializer");
                        if (initializerPrefab != null)
                        {
                            GameObject InitializerObject = Object.Instantiate(initializerPrefab);

                            initializer = InitializerObject.GetComponent<CrystalInitializer>();
                            initializer.Awake();
                            initializer.EnableManualActivation();
                            initializer.LoadGame(false);
                        }
                        else
                        {
                            Debug.LogError("[Game]: CrystalInitializer prefab is missing!");
                        }
                    }
                }
            }
        }
    }
}