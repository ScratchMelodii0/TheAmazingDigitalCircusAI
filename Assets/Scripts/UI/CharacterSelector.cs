using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitalCircus.Core;
using DigitalCircus.Data;

namespace DigitalCircus.UI
{
    /// <summary>
    /// Builds the character-selection grid at runtime from the <see cref="CharacterDatabase"/>,
    /// shows details for the highlighted character, and commits the choice to
    /// <see cref="GameManager"/>. Driven entirely by data so adding a 7th character later
    /// means dropping an asset in the database — no UI work.
    ///
    /// Scene setup (inside the Main Menu's "CharacterSelectPanel"):
    ///   - cardContainer : a panel with a Grid/Horizontal Layout Group; cards are spawned here.
    ///   - cardPrefab    : the CharacterCard prefab (see <see cref="CharacterCardUI"/>).
    ///   - detail labels : name / description / ability / unlock-hint TMP texts.
    ///   - confirmButton : disabled while a locked character is highlighted.
    /// </summary>
    public class CharacterSelector : MonoBehaviour
    {
        [Header("Grid")]
        [SerializeField] private Transform cardContainer;
        [SerializeField] private CharacterCardUI cardPrefab;

        [Header("Detail Panel")]
        [SerializeField] private TMP_Text detailName;
        [SerializeField] private TMP_Text detailDescription;
        [SerializeField] private TMP_Text detailAbility;
        [SerializeField] private TMP_Text detailUnlockHint;
        [SerializeField] private Image detailPortrait;

        [Header("Actions")]
        [Tooltip("Confirms the highlighted character. Disabled when that character is locked.")]
        [SerializeField] private Button confirmButton;

        private readonly List<CharacterCardUI> _cards = new List<CharacterCardUI>();
        private CharacterData _highlighted;

        private void Start()
        {
            BuildGrid();

            if (confirmButton != null)
                confirmButton.onClick.AddListener(ConfirmSelection);
        }

        private void OnDestroy()
        {
            if (confirmButton != null)
                confirmButton.onClick.RemoveListener(ConfirmSelection);
        }

        /// <summary>Instantiates one card per character and highlights the saved choice.</summary>
        private void BuildGrid()
        {
            var gm = GameManager.Instance;
            var db = gm.Database;

            if (db == null || cardContainer == null || cardPrefab == null)
            {
                Debug.LogError("[CharacterSelector] Missing database or grid references — cannot build.");
                return;
            }

            // Clear any design-time placeholder children first.
            foreach (var existing in _cards)
                if (existing != null) Destroy(existing.gameObject);
            _cards.Clear();

            for (int i = 0; i < db.Count; i++)
            {
                CharacterData data = db.GetByIndex(i);
                CharacterCardUI card = Instantiate(cardPrefab, cardContainer);
                bool unlocked = gm.IsCharacterUnlocked(data);
                card.Bind(data, unlocked);
                card.OnClicked = Highlight;
                _cards.Add(card);
            }

            // Start on the player's currently-selected character.
            Highlight(gm.SelectedCharacter ?? db.GetByIndex(0));
        }

        /// <summary>Updates the detail panel and selection frame for the tapped card.</summary>
        private void Highlight(CharacterData data)
        {
            if (data == null) return;
            _highlighted = data;

            bool unlocked = GameManager.Instance.IsCharacterUnlocked(data);

            // Frame the active card, clear the rest.
            foreach (var card in _cards)
                card.SetSelected(card.Data == data);

            if (detailName != null) detailName.text = unlocked ? data.DisplayName : "Locked";
            if (detailDescription != null)
                detailDescription.text = unlocked ? data.Description : data.UnlockHint;
            if (detailAbility != null)
                detailAbility.text = unlocked ? $"Ability: {FormatAbility(data.Ability)}" : string.Empty;
            if (detailUnlockHint != null)
                detailUnlockHint.text = unlocked ? string.Empty : data.UnlockHint;
            if (detailPortrait != null)
            {
                detailPortrait.sprite = data.Portrait;
                detailPortrait.color = unlocked ? Color.white : new Color(0.2f, 0.2f, 0.2f, 1f);
            }

            // Can only confirm an unlocked character.
            if (confirmButton != null) confirmButton.interactable = unlocked;
        }

        /// <summary>Commits the highlighted character and closes the panel.</summary>
        private void ConfirmSelection()
        {
            if (_highlighted == null) return;
            if (!GameManager.Instance.IsCharacterUnlocked(_highlighted)) return;

            GameManager.Instance.SelectCharacter(_highlighted);

            // The MainMenuManager owns navigation; just hide ourselves for now.
            // (Replaced with a proper "ready" callback when the Hub scene is added.)
            gameObject.SetActive(false);
        }

        /// <summary>Turns the AbilityType enum into a friendly label for the UI.</summary>
        private static string FormatAbility(AbilityType ability)
        {
            switch (ability)
            {
                case AbilityType.GlitchDash:   return "Glitch Dash";
                case AbilityType.PatchworkMend:return "Patchwork Mend";
                case AbilityType.WallJumpPrank:return "Wall Jump & Pranks";
                case AbilityType.RibbonGlide:  return "Ribbon Glide";
                case AbilityType.ErraticBlink: return "Erratic Blink";
                case AbilityType.ModularSwap:  return "Modular Swap";
                default:                       return ability.ToString();
            }
        }
    }
}
