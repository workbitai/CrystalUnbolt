using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

namespace GameRebrand
{
    public class RenameFrameworkClasses : EditorWindow
    {
        [MenuItem("Tools/Rename Framework Classes")]
        public static void ShowWindow()
        {
            GetWindow<RenameFrameworkClasses>("Rename Framework");
        }
        
        void OnGUI()
        {
            GUILayout.Label("Framework Class Renaming", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            EditorGUILayout.HelpBox(
                "This will rename framework classes to make them unique:\n\n" +
                "• ScreenManager → ScreenManager\n" +
                "• BaseScreen → BaseScreen\n" +
                "• DataManager → DataManager\n" +
                "• And more...\n\n" +
                "⚠️ BACKUP first!",
                MessageType.Warning
            );
            
            GUILayout.Space(20);
            
            if (GUILayout.Button("🔍 Preview Changes", GUILayout.Height(40)))
            {
                PreviewChanges();
            }
            
            GUILayout.Space(10);
            
            GUI.backgroundColor = Color.yellow;
            if (GUILayout.Button("🔥 RENAME FRAMEWORK CLASSES", GUILayout.Height(50)))
            {
                if (EditorUtility.DisplayDialog(
                    "Confirm Framework Rename",
                    "This will rename core framework classes.\n\n" +
                    "Your game scripts will also be updated to use new names.\n\n" +
                    "Continue?",
                    "YES",
                    "Cancel"))
                {
                    ApplyChanges();
                }
            }
            GUI.backgroundColor = Color.white;
        }
        
        void PreviewChanges()
        {
            var replacements = GetReplacements();
            
            int totalChanges = 0;
            foreach (var pair in replacements)
            {
                string[] files = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    string content = File.ReadAllText(file);
                    totalChanges += Regex.Matches(content, $@"\b{pair.Key}\b").Count;
                }
            }
            
            string message = $"Will rename {replacements.Count} framework classes:\n\n";
            foreach (var pair in replacements)
            {
                message += $"• {pair.Key} → {pair.Value}\n";
            }
            message += $"\nTotal replacements: ~{totalChanges}";
            
            Debug.Log("Preview: " + message);
            EditorUtility.DisplayDialog("Preview", message, "OK");
        }
        
        void ApplyChanges()
        {
            var replacements = GetReplacements();
            string[] allScripts = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
            
            int filesChanged = 0;
            
            for (int i = 0; i < allScripts.Length; i++)
            {
                string file = allScripts[i];
                
                EditorUtility.DisplayProgressBar(
                    "Renaming Framework",
                    $"Processing {Path.GetFileName(file)}",
                    (float)i / allScripts.Length
                );
                
                string content = File.ReadAllText(file);
                string originalContent = content;
                
                // Apply all replacements
                foreach (var pair in replacements)
                {
                    // Use word boundaries to avoid partial matches
                    content = Regex.Replace(content, $@"\b{pair.Key}\b", pair.Value);
                }
                
                if (content != originalContent)
                {
                    File.WriteAllText(file, content);
                    filesChanged++;
                }
            }
            
            EditorUtility.ClearProgressBar();
            
            Debug.Log($"✅ Framework Renamed!\n" +
                     $"Files modified: {filesChanged}\n\n" +
                     $"Unity will now recompile...");
            
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog(
                "Success!",
                $"Framework classes renamed!\n\n" +
                $"Files changed: {filesChanged}\n\n" +
                $"Unity will recompile now.",
                "OK"
            );
        }
        
        System.Collections.Generic.Dictionary<string, string> GetReplacements()
        {
            return new System.Collections.Generic.Dictionary<string, string>()
            {
                // UI System
                {"ScreenManager", "ScreenManager"},
                {"BaseScreen", "BaseScreen"},
                
                // Save System
                {"DataManager", "DataManager"},
                {"SaveData", "SaveData"},
                
                // Currency
                {"EconomyManager", "EconomyManager"},
                
                // Overlay
                {"ScreenOverlay", "ScreenOverlay"},
                
                // Save Area
                {"SafeAreaHandler", "SafeAreaHandler"},
                
                // Pool
                {"ObjectPoolManager", "ObjectPoolManager"},
                
                // Tween
                {"AnimCase", "AnimCase"},
                
                // Audio
                {"SoundManager", "SoundManager"},
                {"MusicPlayer", "MusicPlayer"},
                
                // Module base
                {"GameModule", "GameModule"},
                
                // Common
                {"GameCallback", "GameCallback"}
            };
        }
    }
}






