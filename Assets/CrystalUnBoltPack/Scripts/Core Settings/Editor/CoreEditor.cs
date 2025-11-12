using System.IO;
using UnityEditor;
using UnityEngine;

namespace CrystalUnbolt
{
    [InitializeOnLoad]
    public static class CoreEditor
    {
        // Folders
        public static string FOLDER_CORE { get; private set; }
        public static string FOLDER_CORE_MODULES => Path.Combine(FOLDER_CORE, "Modules");

        public static string FOLDER_DATA;
        public static string FOLDER_SCENES;

        // Editor values
        public static bool UseCustomInspector { get; private set; } = true;
        public static bool UseHierarchyIcons { get; private set; } = true;

        public static bool AutoLoadInitializer { get; private set; } = true;
        public static string InitSceneName { get; private set; } = "Init";

        public static Color AdsDummyBackgroundColor { get; private set; } = new Color(0.2f, 0.2f, 0.3f);
        public static Color AdsDummyMainColor { get; private set; } = new Color(0.2f, 0.3f, 0.7f);

        public static bool ShowFrameworkPromotions { get; private set; } = false;

        static CoreEditor()
        {
            Init();
        }

        private static void Init()
        {
            CoreSettings coreSettings = EditorUtils.GetAsset<CoreSettings>();
            if (coreSettings == null)
            {
                if (EditorApplication.isUpdating || EditorApplication.isCompiling)
                {
                    EditorApplication.delayCall += Init;
                    return;
                }

                Debug.LogWarning("[Game Core]: Core Settings asset cannot be found in the project.");

                coreSettings = ScriptableObject.CreateInstance<CoreSettings>();
                FOLDER_CORE = Path.Combine("Assets", "GameCore");

                if (!AssetDatabase.IsValidFolder(FOLDER_CORE))
                    AssetDatabase.CreateFolder("Assets", "GameCore");

                AssetDatabase.CreateAsset(coreSettings, Path.Combine(FOLDER_CORE, "Core Settings.asset"));
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            else
            {
                FOLDER_CORE = AssetDatabase.GetAssetPath(coreSettings)
                    .Replace(coreSettings.name + ".asset", "");
            }

            ApplySettings(coreSettings);
        }

        public static void ApplySettings(CoreSettings settings)
        {
            FOLDER_DATA = settings.DataFolder;
            FOLDER_SCENES = settings.ScenesFolder;
            InitSceneName = settings.InitSceneName;
            AutoLoadInitializer = settings.AutoLoadInitializer;
            UseCustomInspector = settings.UseCustomInspector;
            UseHierarchyIcons = settings.UseHierarchyIcons;
            AdsDummyBackgroundColor = settings.AdsDummyBackgroundColor;
            AdsDummyMainColor = settings.AdsDummyMainColor;
            ShowFrameworkPromotions = settings.ShowFrameworkPromotions;
        }

        public static string FormatPath(string path)
        {
            return path.Replace("{CORE_MODULES}", FOLDER_CORE_MODULES)
                       .Replace("{CORE_DATA}", FOLDER_DATA)
                       .Replace("{CORE}", FOLDER_CORE);
        }

        public static string GetCoreVersion()
        {
            string coreVersion = null;
            TextAsset coreChangelogText = EditorUtils.GetAsset<TextAsset>("Core Changelog");
            if (coreChangelogText != null && !string.IsNullOrEmpty(coreChangelogText.text))
            {
                string[] lines = coreChangelogText.text.Split('\n');
                if (lines.Length > 0)
                    coreVersion = lines[0];
            }
            return coreVersion;
        }

        // ✅ Renamed from GetTemplateVersion → GetProjectFrameworkVersion
        // ✅ Renamed "Template Changelog" → "Project Framework Changelog"
        public static string GetProjectFrameworkVersion()
        {
            string projectVersion = null;
            TextAsset frameworkChangelogText = EditorUtils.GetAsset<TextAsset>("Project Framework Changelog");
            if (frameworkChangelogText != null && !string.IsNullOrEmpty(frameworkChangelogText.text))
            {
                string[] lines = frameworkChangelogText.text.Split('\n');
                if (lines.Length > 0)
                    projectVersion = lines[0];
            }
            return projectVersion;
        }

        // 🚫 Removed GetDocumentationURL (template reference removed)
    }
}
