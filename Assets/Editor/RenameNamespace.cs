using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

namespace GameRebrand
{
    public class RenameNamespace : EditorWindow
    {
        private string oldNamespace = "CrystalUnbolt";
        private string newNamespace = "YourGameName";
        private string oldPrefix = "Crystal";
        private string newPrefix = "YG"; // YourGame
        
        [MenuItem("Tools/Rebrand Project")]
        public static void ShowWindow()
        {
            GetWindow<RenameNamespace>("Rebrand Project");
        }
        
        void OnGUI()
        {
            GUILayout.Label("Project Rebranding Tool", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            EditorGUILayout.HelpBox(
                "⚠️ WARNING: This will modify ALL C# files!\n" +
                "Make a BACKUP before proceeding!", 
                MessageType.Warning
            );
            
            GUILayout.Space(10);
            
            oldNamespace = EditorGUILayout.TextField("Old Namespace:", oldNamespace);
            newNamespace = EditorGUILayout.TextField("New Namespace:", newNamespace);
            
            GUILayout.Space(5);
            
            oldPrefix = EditorGUILayout.TextField("Old Class Prefix:", oldPrefix);
            newPrefix = EditorGUILayout.TextField("New Class Prefix:", newPrefix);
            
            GUILayout.Space(20);
            
            if (GUILayout.Button("🔍 Preview Changes", GUILayout.Height(40)))
            {
                PreviewChanges();
            }
            
            GUILayout.Space(10);
            
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("🔥 APPLY CHANGES", GUILayout.Height(50)))
            {
                if (EditorUtility.DisplayDialog(
                    "Confirm Rebrand", 
                    $"Replace:\n'{oldNamespace}' → '{newNamespace}'\n'{oldPrefix}' → '{newPrefix}'\n\nContinue?",
                    "YES, DO IT!",
                    "Cancel"))
                {
                    ApplyChanges();
                }
            }
            GUI.backgroundColor = Color.white;
        }
        
        void PreviewChanges()
        {
            string[] allScripts = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
            
            int namespaceCount = 0;
            int classCount = 0;
            
            foreach (string file in allScripts)
            {
                string content = File.ReadAllText(file);
                
                namespaceCount += Regex.Matches(content, $@"\bnamespace\s+{oldNamespace}\b").Count;
                classCount += Regex.Matches(content, $@"\bclass\s+{oldPrefix}\w+").Count;
            }
            
            Debug.Log($"📊 Preview Results:\n" +
                     $"  Files to process: {allScripts.Length}\n" +
                     $"  Namespace changes: {namespaceCount}\n" +
                     $"  Class name changes: {classCount}");
            
            EditorUtility.DisplayDialog(
                "Preview", 
                $"Found:\n" +
                $"• {allScripts.Length} C# files\n" +
                $"• {namespaceCount} namespace declarations\n" +
                $"• {classCount} class names to change",
                "OK"
            );
        }
        
        void ApplyChanges()
        {
            string[] allScripts = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
            
            int filesChanged = 0;
            int totalChanges = 0;
            
            EditorUtility.DisplayProgressBar("Rebranding", "Processing files...", 0);
            
            for (int i = 0; i < allScripts.Length; i++)
            {
                string file = allScripts[i];
                
                EditorUtility.DisplayProgressBar(
                    "Rebranding", 
                    $"Processing {Path.GetFileName(file)}", 
                    (float)i / allScripts.Length
                );
                
                string content = File.ReadAllText(file);
                string originalContent = content;
                
                // Replace namespace
                content = Regex.Replace(
                    content, 
                    $@"\bnamespace\s+{oldNamespace}\b", 
                    $"namespace {newNamespace}"
                );
                
                // Replace using statements
                content = Regex.Replace(
                    content, 
                    $@"\busing\s+{oldNamespace}\b", 
                    $"using {newNamespace}"
                );
                
                // Replace class names (careful - only at class declaration)
                content = Regex.Replace(
                    content,
                    $@"\b(public|private|internal|protected)\s+(class|interface|struct)\s+{oldPrefix}",
                    $"$1 $2 {newPrefix}"
                );
                
                if (content != originalContent)
                {
                    File.WriteAllText(file, content);
                    filesChanged++;
                    totalChanges += CountChanges(originalContent, content);
                }
            }
            
            EditorUtility.ClearProgressBar();
            
            Debug.Log($"✅ Rebranding Complete!\n" +
                     $"  Files modified: {filesChanged}\n" +
                     $"  Total changes: {totalChanges}\n\n" +
                     $"Please wait for Unity to recompile...");
            
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog(
                "Success!", 
                $"Rebranding completed!\n\n" +
                $"Files changed: {filesChanged}\n" +
                $"Total replacements: {totalChanges}\n\n" +
                $"Unity will now recompile scripts.",
                "OK"
            );
        }
        
        int CountChanges(string original, string modified)
        {
            int count = 0;
            for (int i = 0; i < original.Length && i < modified.Length; i++)
            {
                if (original[i] != modified[i]) count++;
            }
            return count / 10; // Approximate number of changes
        }
    }
}






