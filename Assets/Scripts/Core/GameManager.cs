using System;
using UnityEngine;
using DigitalCircus.Data;

namespace DigitalCircus.Core
{
    /// <summary>
    /// The single persistent object that survives scene loads and owns the live game state:
    /// the loaded save, the character database, and the currently selected character.
    ///
    /// Implemented as a lazily-created singleton so any scene (Main Menu, Hub, a level)
    /// can call <see cref="GameManager.Instance"/> without needing a pre-placed object.
    /// This is what carries the player's choice from the selection menu into the Hub/levels.
    /// </summary>
    [DefaultExecutionOrder(-100)] // Initialize before normal MonoBehaviours that read state.
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;

        /// <summary>
        /// Global access point. Creates the manager on first use so the game works even
        /// if you press Play directly in a scene that has no manager placed.
        /// </summary>
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[GameManager]");
                    _instance = go.AddComponent<GameManager>();
                    DontDestroyOnLoad(go);
                    _instance.Initialize();
                }
                return _instance;
            }
        }

        /// <summary>Raised whenever the selected character changes (menus/HUD listen to refresh).</summary>
        public event Action<CharacterData> OnSelectedCharacterChanged;

        public SaveData Save { get; private set; }
        public CharacterDatabase Database { get; private set; }
        public CharacterData SelectedCharacter { get; private set; }

        private bool _initialized;

        private void Awake()
        {
            // Enforce the singleton if an instance was placed in a scene manually.
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }

        private void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            Database = CharacterDatabase.Load();
            Save = SaveManager.Load();

            // Resolve the saved character id to a concrete asset, falling back to the
            // first roster entry if the save is empty or references a removed character.
            if (Database != null)
            {
                SelectedCharacter = Database.GetById(Save.selectedCharacterId)
                                    ?? Database.GetByIndex(0);
                if (SelectedCharacter != null)
                    Save.selectedCharacterId = SelectedCharacter.CharacterId;
            }
        }

        /// <summary>Sets the active character and persists the choice immediately.</summary>
        public void SelectCharacter(CharacterData character)
        {
            if (character == null) return;

            SelectedCharacter = character;
            Save.selectedCharacterId = character.CharacterId;
            SaveManager.Save(Save);
            OnSelectedCharacterChanged?.Invoke(character);
        }

        /// <summary>Has the player unlocked this character (or is it free by default)?</summary>
        public bool IsCharacterUnlocked(CharacterData character)
        {
            if (character == null) return false;
            if (character.UnlockedByDefault) return true;
            return Save.unlockedCharacters.Contains(character.CharacterId);
        }

        /// <summary>Unlocks a character and persists it (called from level completion later).</summary>
        public void UnlockCharacter(string characterId)
        {
            if (string.IsNullOrEmpty(characterId)) return;
            if (!Save.unlockedCharacters.Contains(characterId))
            {
                Save.unlockedCharacters.Add(characterId);
                SaveManager.Save(Save);
            }
        }

        /// <summary>Persists the current save (call on pause / level transitions).</summary>
        public void PersistSave() => SaveManager.Save(Save);

        // Mobile lifecycle: save when the app is backgrounded so progress is never lost
        // if the OS kills the process while suspended.
        private void OnApplicationPause(bool paused)
        {
            if (paused) PersistSave();
        }

        private void OnApplicationQuit() => PersistSave();
    }
}
