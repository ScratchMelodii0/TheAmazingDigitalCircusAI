using System;
using System.IO;
using UnityEngine;

namespace DigitalCircus.Core
{
    /// <summary>
    /// Handles persisting <see cref="SaveData"/> to disk as JSON.
    ///
    /// We write to <c>Application.persistentDataPath</c> because it is the only
    /// reliably writable location on both Android and iOS. JSON (over PlayerPrefs)
    /// keeps the whole game state in one human-readable file that is easy to debug
    /// and to extend as the game grows.
    ///
    /// This is a static utility — it owns no scene state. The live, mutable copy of
    /// the save lives on <see cref="GameManager"/>.
    /// </summary>
    public static class SaveManager
    {
        private const string FileName = "circus_save.json";

        // Cached because Application.persistentDataPath cannot be read off the main thread.
        private static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        /// <summary>True if a save file already exists (used to toggle the Continue button).</summary>
        public static bool SaveExists()
        {
            try { return File.Exists(FilePath); }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] SaveExists check failed: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Serializes and writes the given data. Returns false on failure so callers
        /// can surface a non-fatal "couldn't save" message instead of crashing.
        /// </summary>
        public static bool Save(SaveData data)
        {
            if (data == null)
            {
                Debug.LogError("[SaveManager] Refusing to save null data.");
                return false;
            }

            try
            {
                data.lastSavedUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                string json = JsonUtility.ToJson(data, prettyPrint: true);

                // Write to a temp file first, then atomically replace, so a crash
                // mid-write can never corrupt an existing good save.
                string tempPath = FilePath + ".tmp";
                File.WriteAllText(tempPath, json);
                if (File.Exists(FilePath)) File.Delete(FilePath);
                File.Move(tempPath, FilePath);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to save: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Loads and deserializes the save. If the file is missing or corrupt, returns
        /// a fresh default <see cref="SaveData"/> so the game is always playable.
        /// </summary>
        public static SaveData Load()
        {
            if (!SaveExists())
                return CreateDefault();

            try
            {
                string json = File.ReadAllText(FilePath);
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                if (data == null)
                {
                    Debug.LogWarning("[SaveManager] Save parsed to null — using defaults.");
                    return CreateDefault();
                }
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to load (using defaults): {e.Message}");
                return CreateDefault();
            }
        }

        /// <summary>Deletes the save file (used by Settings ▸ Reset Progress).</summary>
        public static bool DeleteSave()
        {
            try
            {
                if (File.Exists(FilePath)) File.Delete(FilePath);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to delete save: {e.Message}");
                return false;
            }
        }

        /// <summary>A brand-new game state: Pomni unlocked, Pilot available, full sanity.</summary>
        public static SaveData CreateDefault()
        {
            var data = new SaveData();
            data.unlockedCharacters.Add("pomni");
            return data;
        }
    }
}
