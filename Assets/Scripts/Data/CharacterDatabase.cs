using System.Collections.Generic;
using UnityEngine;

namespace DigitalCircus.Data
{
    /// <summary>
    /// A single asset that lists every playable <see cref="CharacterData"/> in roster order.
    /// Create one via: Assets ▸ Create ▸ Digital Circus ▸ Character Database
    /// then drag all six character assets into the list.
    ///
    /// Placing it under Assets/Resources/Data lets any scene load it without a manual
    /// inspector reference (see <see cref="Load"/>), which keeps scene wiring simple.
    /// </summary>
    [CreateAssetMenu(
        fileName = "CharacterDatabase",
        menuName = "Digital Circus/Character Database",
        order = 1)]
    public class CharacterDatabase : ScriptableObject
    {
        [Tooltip("All playable characters, in the order they appear on the selection screen.")]
        [SerializeField] private List<CharacterData> characters = new List<CharacterData>();

        /// <summary>Resource path (relative to a Resources folder, no extension).</summary>
        public const string ResourcePath = "Data/CharacterDatabase";

        public IReadOnlyList<CharacterData> Characters => characters;
        public int Count => characters.Count;

        public CharacterData GetByIndex(int index)
        {
            if (index < 0 || index >= characters.Count)
            {
                Debug.LogError($"[CharacterDatabase] Index {index} out of range (count {characters.Count}).", this);
                return null;
            }
            return characters[index];
        }

        /// <summary>
        /// Finds a character by its stable id. Returns null (with a warning) if missing,
        /// which lets the save system fall back to the default character gracefully.
        /// </summary>
        public CharacterData GetById(string characterId)
        {
            if (string.IsNullOrEmpty(characterId)) return null;

            for (int i = 0; i < characters.Count; i++)
            {
                if (characters[i] != null && characters[i].CharacterId == characterId)
                    return characters[i];
            }
            Debug.LogWarning($"[CharacterDatabase] No character with id '{characterId}'.", this);
            return null;
        }

        public int IndexOf(string characterId)
        {
            for (int i = 0; i < characters.Count; i++)
            {
                if (characters[i] != null && characters[i].CharacterId == characterId)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Convenience loader so menus don't need a serialized reference.
        /// Caches nothing — callers should hold the reference they get back.
        /// </summary>
        public static CharacterDatabase Load()
        {
            var db = Resources.Load<CharacterDatabase>(ResourcePath);
            if (db == null)
            {
                Debug.LogError(
                    $"[CharacterDatabase] Could not load from 'Resources/{ResourcePath}'. " +
                    "Create the asset and place it under Assets/Resources/Data/.");
            }
            return db;
        }
    }
}
