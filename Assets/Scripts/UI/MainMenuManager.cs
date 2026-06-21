using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DigitalCircus.Core;

namespace DigitalCircus.UI
{
    /// <summary>
    /// Top-level controller for the Main Menu scene. Owns simple panel navigation
    /// (Home ▸ Character Select ▸ Settings) and the buttons that launch the game.
    ///
    /// Scene setup (MainMenu.unity):
    ///   Canvas (Screen Space - Overlay, Canvas Scaler = "Scale With Screen Size", 1080x1920)
    ///     - HomePanel        : Play / Continue / Settings / Quit buttons
    ///     - CharacterSelectPanel : holds the CharacterSelector
    ///     - SettingsPanel    : added in the settings step
    /// Attach this to a "MenuRoot" object and wire the serialized fields.
    ///
    /// Keeping all navigation here (rather than per-button UnityEvents) means the flow
    /// is readable in one file and easy to extend when the Hub scene is introduced.
    /// </summary>
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject homePanel;
        [SerializeField] private GameObject characterSelectPanel;
        [SerializeField] private GameObject settingsPanel;

        [Header("Home Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("Navigation Buttons")]
        [SerializeField] private Button characterSelectBackButton;
        [SerializeField] private Button settingsBackButton;
        [SerializeField] private Button startAdventureButton; // shown after a character is chosen

        [Header("Scene Flow")]
        [Tooltip("Name of the Hub scene to load. Must be added to Build Settings.")]
        [SerializeField] private string hubSceneName = "Hub";

        private void Awake()
        {
            // Touching Instance here forces the persistent GameManager to spin up and
            // load the save before any menu logic runs.
            _ = GameManager.Instance;
        }

        private void Start()
        {
            WireButtons();
            ShowHome();
        }

        private void WireButtons()
        {
            if (playButton != null) playButton.onClick.AddListener(ShowCharacterSelect);
            if (continueButton != null) continueButton.onClick.AddListener(ContinueGame);
            if (settingsButton != null) settingsButton.onClick.AddListener(ShowSettings);
            if (quitButton != null) quitButton.onClick.AddListener(QuitGame);

            if (characterSelectBackButton != null) characterSelectBackButton.onClick.AddListener(ShowHome);
            if (settingsBackButton != null) settingsBackButton.onClick.AddListener(ShowHome);
            if (startAdventureButton != null) startAdventureButton.onClick.AddListener(LoadHub);

            // "Continue" is only meaningful if a save already exists.
            if (continueButton != null)
                continueButton.interactable = SaveManager.SaveExists();
        }

        // ---- Panel navigation ----------------------------------------------------

        public void ShowHome()
        {
            SetPanel(homePanel, true);
            SetPanel(characterSelectPanel, false);
            SetPanel(settingsPanel, false);
        }

        public void ShowCharacterSelect()
        {
            SetPanel(homePanel, false);
            SetPanel(characterSelectPanel, true);
            SetPanel(settingsPanel, false);
        }

        public void ShowSettings()
        {
            SetPanel(homePanel, false);
            SetPanel(characterSelectPanel, false);
            SetPanel(settingsPanel, true);
        }

        // ---- Actions -------------------------------------------------------------

        /// <summary>Continue uses the already-selected character and jumps to the Hub.</summary>
        private void ContinueGame()
        {
            if (!SaveManager.SaveExists()) return;
            LoadHub();
        }

        /// <summary>Loads the Hub scene, guarding against a missing Build Settings entry.</summary>
        private void LoadHub()
        {
            GameManager.Instance.PersistSave();

            if (Application.CanStreamedLevelBeLoaded(hubSceneName))
            {
                SceneManager.LoadScene(hubSceneName);
            }
            else
            {
                // The Hub scene arrives in step 3 — until then, log clearly instead of crashing.
                Debug.LogWarning(
                    $"[MainMenuManager] Scene '{hubSceneName}' is not in Build Settings yet. " +
                    "Add it under File ▸ Build Settings once the Hub scene exists.");
            }
        }

        private void QuitGame()
        {
            GameManager.Instance.PersistSave();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private static void SetPanel(GameObject panel, bool visible)
        {
            if (panel != null) panel.SetActive(visible);
        }
    }
}
