using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace CrystalUnbolt
{
    public static class Serializer
    {
        private static string persistentDataPath;

        public static void Init()
        {
            persistentDataPath = Application.persistentDataPath;
        }

        /// <summary>
        /// Deserializes file located at Persistent Data Path.
        /// </summary>
        /// <param name="fileName">Name of input file.</param>
        /// <returns>Deserialized object if file exists or new instance if doesn't.</returns>
        public static T Deserialize<T>(string fileName, bool logIfFileNotExists = false) where T : new()
        {
            string absolutePath = Path.Combine(GetPersistentDataPath(), fileName);

            if (FileExistsAtPath(absolutePath))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = null;

                try
                {
                    file = File.Open(absolutePath, FileMode.Open);

                    // Try deserializing
                    T deserializedObject = (T)bf.Deserialize(file);

                    if (deserializedObject == null)
                    {
                        Debug.LogWarning($"[Serializer] Deserialized object is null: {absolutePath}");
                        return new T();
                    }

                    return deserializedObject;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[Serializer] Failed to load save file '{absolutePath}' - {ex.Message}");

                    // Close and delete corrupt file
                    if (file != null)
                        file.Close();

                    try
                    {
                        File.Delete(absolutePath);
                        Debug.LogWarning($"[Serializer] Corrupt save deleted: {absolutePath}");
                    }
                    catch (Exception deleteEx)
                    {
                        Debug.LogWarning($"[Serializer] Failed to delete corrupt file: {deleteEx.Message}");
                    }

                    return new T();
                }
                finally
                {
                    if (file != null)
                        file.Close();
                }
            }
            else
            {
                if (logIfFileNotExists)
                {
                    Debug.LogWarning($"[Serializer] File at path \"{absolutePath}\" does not exist.");
                }

                return new T();
            }
        }

        /// <summary>
        /// Serializes file to Persistent Data Path.
        /// </summary>
        public static void Serialize<T>(T objectToSerialize, string fileName)
        {
            try
            {
                string fullPath = Path.Combine(GetPersistentDataPath(), fileName);

                BinaryFormatter bf = new BinaryFormatter();
                using (FileStream file = File.Open(fullPath, FileMode.Create))
                {
                    bf.Serialize(file, objectToSerialize);
                }

                Debug.Log($"[Serializer] File saved successfully: {fullPath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Serializer] Failed to save file '{fileName}' - {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if file exists at Persistent Data Path.
        /// </summary>
        public static bool FileExistsAtPDP(string fileName)
        {
            return File.Exists(Path.Combine(GetPersistentDataPath(), fileName));
        }

        /// <summary>
        /// Checks if file exists at Persistent Data Path.
        /// </summary>
        public static bool FileExistsAtPath(string absolutePath)
        {
            return File.Exists(absolutePath);
        }

        /// <summary>
        /// Checks if file exists at specified directory.
        /// </summary>
        public static bool FileExistsAtPath(string directoryPath, string fileName)
        {
            return File.Exists(Path.Combine(directoryPath, fileName));
        }

        /// <summary>
        /// Delete file at Persistent Data Path.
        /// </summary>
        public static void DeleteFileAtPDP(string fileName)
        {
            try
            {
                string fullPath = Path.Combine(GetPersistentDataPath(), fileName);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    Debug.Log($"[Serializer] Deleted file: {fullPath}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Serializer] Failed to delete file {fileName}: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete file at specified path.
        /// </summary>
        public static void DeleteFileAtPath(string absolutePath)
        {
            try
            {
                if (File.Exists(absolutePath))
                    File.Delete(absolutePath);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Serializer] Failed to delete file: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete file at specified directory + filename.
        /// </summary>
        public static void DeleteFileAtPath(string directoryPath, string fileName)
        {
            try
            {
                string fullPath = Path.Combine(directoryPath, fileName);
                if (File.Exists(fullPath))
                    File.Delete(fullPath);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Serializer] Failed to delete file: {ex.Message}");
            }
        }

        private static string GetPersistentDataPath()
        {
            if (string.IsNullOrEmpty(persistentDataPath))
            {
                persistentDataPath = Application.persistentDataPath;
                Debug.Log($"[Serializer] Persistent Data Path initialized: {persistentDataPath}");
            }

            return persistentDataPath;
        }
    }
}
