using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitalCircus.Core;
using DigitalCircus.Data;

namespace DigitalCircus.UI
{
    /// <summary>
    /// Controller for the Circus Hub scene — the central tent the player returns to
    /// between chapters. It displays the active character and the sanity meter, hosts the
    /// character-select and episode-select panels, and launches a chosen episode through
    /// the async <see cref="SceneLoader"/>.
    ///
    /// Scene setup (Hub.unity):
    ///   Canvas (1080×1920, Scale With Screen Size)
    ///     ├── HubRoot            : character display, SanityMeter, "Select Episode" /
    ///     │                        "Change Character" / Settings buttons
    ///     ├── CharacterSelectPanel : reuses CharacterSelector (from Step 1)
    ///     ├── EpisodeSelectPanel   : hosts EpisodeSelector + a Back button
    ///     └── LoadingPanel         : full-screen overlay + progress bar (shown while loading)
    /// Add this to a "HubManager" object and wire the fields.
    /// </summary>
    public class HubManager : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject hubRoot;
        [SerializeField] private GameObject characterSelectPanel;
        [SerializeField] private GameObject episodeSelectPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject loadingPanel;

        [Header("Character Display")]
        [SerializeField] private TMP_Text characterNameLabel;
        [SerializeField] private Image characterPortrait;
        [SerializeField] private TMP_Text welcomeLabel;

        [Header("Episode Flow")]
        [SerializeField] private EpisodeSelector episodeSelector;
        [SerializeField] private Slider loadingProgressBar;

        [Header("Hub Buttons")]
        [SerializeField] private Button selectEpisodeButton;
        [SerializeField] private Button changeCharacterButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button backToMenuButton;

        [Header("Navigation Buttons")]
        [SerializeField] private Button characterSelectBackButton;
        [SerializeField] private Button episodeSelectBackButton;
        [SerializeField] private Button settingsBackButton;

        [Header("Scene Flow")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        [Header("Flavor")]
        [Tooltip("Caine-style greeting lines shown at random when entering the Hub.")]
        [SerializeField]
        private string[] caineGreetings =
        {
            "Welcome back to the Amazing Digital Circus!",
            "Ready for another WONDERFUL adventure?",
            "Don't worry about the exit. Focus on the FUN!",
            "Try not to abstract today, alright champ?"
        };

        private GameManager _gm;

        private void Start()
        {
            _gm = GameManager.Instance;

            // Returning to the safety of the tent restores sanity.
            _gm.ResetSanity();

            WireButtons();
            RefreshCharacterDisplay(_gm.SelectedCharacter);
            ShowGreeting();

            _gm.OnSelectedCharacterChanged += RefreshCharacterDisplay;
            if (episodeSelector != null)
                episodeSelector.OnEpisodeConfirmed += LaunchEpisode;

            ShowHub();
        }

        private void OnDestroy()
        {
            if (_gm != null) _gm.OnSelectedCharacterChanged -= RefreshCharacterDisplay;
            if (episodeSelector != null) episodeSelector.OnEpisodeConfirmed -= LaunchEpisode;
        }

        private void WireButtons()
        {
            if (selectEpisodeButton != null) selectEpisodeButton.onClick.AddListener(ShowEpisodeSelect);
            if (changeCharacterButton != null) changeCharacterButton.onClick.AddListener(ShowCharacterSelect);
            if (settingsButton != null) settingsButton.onClick.AddListener(ShowSettings);
            if (backToMenuButton != null) backToMenuButton.onClick.AddListener(BackToMenu);

            if (characterSelectBackButton != null) characterSelectBackButton.onClick.AddListener(ReturnFromCharacterSelect);
            if (episodeSelectBackButton != null) episodeSelectBackButton.onClick.AddListener(ShowHub);
            if (settingsBackButton != null) settingsBackButton.onClick.AddListener(ShowHub);
        }

        // ---- Display -------------------------------------------------------------

        private void RefreshCharacterDisplay(CharacterData character)
        {
            if (character == null) return;
            if (characterNameLabel != null) characterNameLabel.text = character.DisplayName;
            if (characterPortrait != null)
            {
                characterPortrait.sprite = character.Portrait;
                characterPortrait.color = character.Portrait != null ? Color.white : character.ThemeColor;
            }
        }

        private void ShowGreeting()
        {
            if (welcomeLabel == null || caineGreetings == null || caineGreetings.Length == 0) return;
            welcomeLabel.text = caineGreetings[Random.Range(0, caineGreetings.Length)];
        }

        // ---- Panel navigation ----------------------------------------------------

        public void ShowHub() => SetPanels(hub: true);
        public void ShowCharacterSelect() => SetPanels(character: true);
        public void ShowEpisodeSelect() => SetPanels(episode: true);
        public void ShowSettings() => SetPanels(settings: true);

        /// <summary>Character-select panel hides itself on confirm, so refresh + return home.</summary>
        private void ReturnFromCharacterSelect()
        {
            if (characterSelectPanel != null) characterSelectPanel.SetActive(true);
            RefreshCharacterDisplay(_gm.SelectedCharacter);
            ShowHub();
        }

        private void SetPanels(bool hub = false, bool character = false,
            bool episode = false, bool settings = false, bool loading = false)
        {
            if (hubRoot != null) hubRoot.SetActive(hub);
            if (characterSelectPanel != null) characterSelectPanel.SetActive(character);
            if (episodeSelectPanel != null) episodeSelectPanel.SetActive(episode);
            if (settingsPanel != null) settingsPanel.SetActive(settings);
            if (loadingPanel != null) loadingPanel.SetActive(loading);
        }

        // ---- Actions -------------------------------------------------------------

        private void LaunchEpisode(EpisodeData episode)
        {
            if (episode == null) return;

            SetPanels(loading: true);
            if (loadingProgressBar != null) loadingProgressBar.value = 0f;

            // Fresh sanity for the run; the level's own systems take over from here.
            _gm.ResetSanity();

            SceneLoader.Instance.Load(
                episode.SceneName,
                onProgress: p => { if (loadingProgressBar != null) loadingProgressBar.value = p; });
        }

        private void BackToMenu()
        {
            _gm.PersistSave();
            SceneLoader.Instance.Load(mainMenuSceneName);
        }
    }
}
