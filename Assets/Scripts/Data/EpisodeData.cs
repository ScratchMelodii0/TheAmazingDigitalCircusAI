using UnityEngine;

namespace DigitalCircus.Data
{
    /// <summary>
    /// Data-driven definition of one chapter/episode (Pilot → The Last Act).
    /// Create via: Assets ▸ Create ▸ Digital Circus ▸ Episode Data.
    ///
    /// Keeping episodes as assets lets the Hub's episode-select grid build itself and
    /// lets designers retitle/reorder chapters or point one at a different scene without
    /// touching code.
    /// </summary>
    [CreateAssetMenu(
        fileName = "NewEpisode",
        menuName = "Digital Circus/Episode Data",
        order = 2)]
    public class EpisodeData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Stable id used by the save system. Never rename once shipped.")]
        [SerializeField] private string episodeId = "ep01_pilot";
        [Tooltip("1-based number shown on the card (1 = Pilot).")]
        [Min(1)]
        [SerializeField] private int episodeNumber = 1;
        [SerializeField] private string title = "Pilot";
        [SerializeField] private string subtitle = "There's an exit somewhere...";
        [TextArea(2, 4)]
        [SerializeField] private string description =
            "Pomni wakes in the circus. Find the exit door while the Gloinks swarm.";

        [Header("Scene")]
        [Tooltip("Scene name to load for this episode. Must be added to Build Settings.")]
        [SerializeField] private string sceneName = "Episode01";

        [Header("Presentation")]
        [Tooltip("Card thumbnail. Placeholder: a colored square.")]
        [SerializeField] private Sprite thumbnail;
        [SerializeField] private Color themeColor = new Color(0.9f, 0.3f, 0.5f);

        [Header("Unlock")]
        [Tooltip("True only for the Pilot — the rest unlock by completing the previous one.")]
        [SerializeField] private bool unlockedByDefault = false;

        public string EpisodeId => episodeId;
        public int EpisodeNumber => episodeNumber;
        public string Title => title;
        public string Subtitle => subtitle;
        public string Description => description;
        public string SceneName => sceneName;
        public Sprite Thumbnail => thumbnail;
        public Color ThemeColor => themeColor;
        public bool UnlockedByDefault => unlockedByDefault;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(episodeId))
                Debug.LogWarning($"[EpisodeData] '{name}' has an empty episodeId.", this);
            if (string.IsNullOrWhiteSpace(sceneName))
                Debug.LogWarning($"[EpisodeData] '{name}' has no sceneName set.", this);
        }
#endif
    }
}
