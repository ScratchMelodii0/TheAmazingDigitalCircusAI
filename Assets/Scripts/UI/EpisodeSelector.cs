using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitalCircus.Core;
using DigitalCircus.Data;

namespace DigitalCircus.UI
{
    /// <summary>
    /// Builds the Hub's episode-select list from the <see cref="EpisodeDatabase"/>, shows
    /// the highlighted episode's details, and reports the chosen episode back to the Hub
    /// (which performs the actual scene load). Locked episodes are visible but not playable.
    /// </summary>
    public class EpisodeSelector : MonoBehaviour
    {
        [Header("List")]
        [SerializeField] private Transform cardContainer;
        [SerializeField] private EpisodeCardUI cardPrefab;

        [Header("Detail Panel")]
        [SerializeField] private TMP_Text detailTitle;
        [SerializeField] private TMP_Text detailSubtitle;
        [SerializeField] private TMP_Text detailDescription;
        [SerializeField] private Image detailThumbnail;

        [Header("Action")]
        [Tooltip("Launches the highlighted episode. Disabled while a locked episode is shown.")]
        [SerializeField] private Button playButton;

        /// <summary>Raised when the player confirms a playable episode (Hub loads its scene).</summary>
        public event Action<EpisodeData> OnEpisodeConfirmed;

        private readonly List<EpisodeCardUI> _cards = new List<EpisodeCardUI>();
        private EpisodeData _highlighted;

        private void Start()
        {
            BuildList();
            if (playButton != null) playButton.onClick.AddListener(ConfirmSelection);
        }

        private void OnDestroy()
        {
            if (playButton != null) playButton.onClick.RemoveListener(ConfirmSelection);
        }

        private void BuildList()
        {
            var gm = GameManager.Instance;
            var db = gm.Episodes;
            if (db == null || cardContainer == null || cardPrefab == null)
            {
                Debug.LogError("[EpisodeSelector] Missing database or list references.");
                return;
            }

            foreach (var c in _cards) if (c != null) Destroy(c.gameObject);
            _cards.Clear();

            for (int i = 0; i < db.Count; i++)
            {
                EpisodeData data = db.GetByIndex(i);
                EpisodeCardUI card = Instantiate(cardPrefab, cardContainer);
                card.Bind(data, gm.IsEpisodeUnlocked(i));
                card.OnClicked = Highlight;
                _cards.Add(card);
            }

            Highlight(gm.SelectedEpisode ?? db.GetByIndex(0));
        }

        private void Highlight(EpisodeData data)
        {
            if (data == null) return;
            _highlighted = data;

            bool unlocked = GameManager.Instance.IsEpisodeUnlocked(data);

            foreach (var card in _cards)
                card.SetSelected(card.Data == data);

            if (detailTitle != null) detailTitle.text = $"Ep {data.EpisodeNumber}: {data.Title}";
            if (detailSubtitle != null) detailSubtitle.text = unlocked ? data.Subtitle : string.Empty;
            if (detailDescription != null)
                detailDescription.text = unlocked ? data.Description : "Complete the previous episode to unlock.";
            if (detailThumbnail != null)
            {
                detailThumbnail.sprite = data.Thumbnail;
                detailThumbnail.color = unlocked ? Color.white : new Color(0.2f, 0.2f, 0.2f, 1f);
            }

            if (playButton != null) playButton.interactable = unlocked;
        }

        private void ConfirmSelection()
        {
            if (_highlighted == null) return;
            if (!GameManager.Instance.IsEpisodeUnlocked(_highlighted)) return;

            GameManager.Instance.SelectEpisode(_highlighted);
            OnEpisodeConfirmed?.Invoke(_highlighted);
        }
    }
}
