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

        /// <summary>Maximum value of the sanity / abstraction meter.</summary>
        public const float MaxSanity = 100f;

        /// <summary>Raised whenever the selected character changes (menus/HUD listen to refresh).</summary>
        public event Action<CharacterData> OnSelectedCharacterChanged;

        /// <summary>Raised whenever the active episode changes.</summary>
        public event Action<EpisodeData> OnSelectedEpisodeChanged;

        /// <summary>Raised whenever sanity changes (current, max). Drives the HUD meter.</summary>
        public event Action<float, float> OnSanityChanged;

        /// <summary>Raised once when sanity hits zero — the player has "abstracted".</summary>
        public event Action OnAbstracted;

        public SaveData Save { get; private set; }
        public CharacterDatabase Database { get; private set; }
        public EpisodeDatabase Episodes { get; private set; }
        public CharacterData SelectedCharacter { get; private set; }
        public EpisodeData SelectedEpisode { get; private set; }

        /// <summary>Live sanity value, 0…<see cref="MaxSanity"/>.</summary>
        public float Sanity => Save != null ? Save.sanity : MaxSanity;

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
            Episodes = EpisodeDatabase.Load();
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

            // Default the active episode to the highest one the player has reached.
            if (Episodes != null && Episodes.Count > 0)
            {
                int idx = Mathf.Clamp(Save.highestUnlockedEpisode, 0, Episodes.Count - 1);
                SelectedEpisode = Episodes.GetByIndex(idx);
            }
        }

        // ---- Episodes ------------------------------------------------------------

        /// <summary>Sets the active episode (does not load the scene — the Hub does that).</summary>
        public void SelectEpisode(EpisodeData episode)
        {
            if (episode == null) return;
            SelectedEpisode = episode;
            OnSelectedEpisodeChanged?.Invoke(episode);
        }

        /// <summary>Is this episode reachable given the player's progress?</summary>
        public bool IsEpisodeUnlocked(int episodeIndex) =>
            episodeIndex <= Save.highestUnlockedEpisode;

        public bool IsEpisodeUnlocked(EpisodeData episode)
        {
            if (episode == null || Episodes == null) return false;
            if (episode.UnlockedByDefault) return true;
            int idx = IndexOfEpisode(episode);
            return idx >= 0 && idx <= Save.highestUnlockedEpisode;
        }

        private int IndexOfEpisode(EpisodeData episode)
        {
            for (int i = 0; i < Episodes.Count; i++)
                if (Episodes.GetByIndex(i) == episode) return i;
            return -1;
        }

        /// <summary>Marks an episode complete, unlocking the next one, and persists.</summary>
        public void CompleteEpisode(int episodeIndex)
        {
            int next = episodeIndex + 1;
            if (next > Save.highestUnlockedEpisode)
            {
                Save.highestUnlockedEpisode = next;
                SaveManager.Save(Save);
            }
        }

        // ---- Sanity / Abstraction ------------------------------------------------

        /// <summary>Adds to (or subtracts from) sanity, clamps, persists nothing (use during play).</summary>
        public void ModifySanity(float delta)
        {
            if (Save == null) return;
            float previous = Save.sanity;
            Save.sanity = Mathf.Clamp(Save.sanity + delta, 0f, MaxSanity);

            if (!Mathf.Approximately(previous, Save.sanity))
                OnSanityChanged?.Invoke(Save.sanity, MaxSanity);

            if (Save.sanity <= 0f && previous > 0f)
                OnAbstracted?.Invoke();
        }

        /// <summary>Restores sanity to full (e.g. when entering the Hub or starting a level).</summary>
        public void ResetSanity()
        {
            if (Save == null) return;
            Save.sanity = MaxSanity;
            OnSanityChanged?.Invoke(Save.sanity, MaxSanity);
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
