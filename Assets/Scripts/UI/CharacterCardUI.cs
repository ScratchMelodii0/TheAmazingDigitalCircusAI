using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using DigitalCircus.Data;

namespace DigitalCircus.UI
{
    /// <summary>
    /// Visual + interaction for one character "card" in the selection grid.
    ///
    /// Prefab setup (Assets/Prefabs/CharacterCard.prefab):
    ///   - Root: Button (this script lives here)
    ///       - PortraitImage   (Image)  → assign to portraitImage
    ///       - NameLabel       (TMP_Text)
    ///       - LockOverlay     (GameObject, an Image + lock icon, hidden when unlocked)
    ///       - SelectionFrame  (GameObject, a highlight outline, shown when selected)
    /// Placeholder art: use Unity's built-in "UISprite" (the default knob) or a
    /// solid color square for the portrait until real sprites are dropped in.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class CharacterCardUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image portraitImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TMP_Text nameLabel;
        [SerializeField] private GameObject lockOverlay;
        [SerializeField] private GameObject selectionFrame;

        private Button _button;
        private CharacterData _data;
        private bool _unlocked;

        /// <summary>Fired with this card's data when a tap selects an unlocked character.</summary>
        public UnityAction<CharacterData> OnClicked;

        public CharacterData Data => _data;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(HandleClick);
        }

        private void OnDestroy()
        {
            if (_button != null) _button.onClick.RemoveListener(HandleClick);
        }

        /// <summary>Populates the card from a character asset and its unlock state.</summary>
        public void Bind(CharacterData data, bool unlocked)
        {
            _data = data;
            _unlocked = unlocked;

            if (data == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            if (nameLabel != null)
                nameLabel.text = unlocked ? data.DisplayName : "???";

            if (portraitImage != null)
            {
                portraitImage.sprite = data.Portrait;
                // Dim locked characters; tint with the theme color when unlocked.
                portraitImage.color = unlocked ? Color.white : new Color(0.2f, 0.2f, 0.2f, 1f);
                // Keep the box visible even before art exists.
                portraitImage.enabled = true;
            }

            if (backgroundImage != null)
                backgroundImage.color = unlocked ? data.ThemeColor : Color.gray;

            if (lockOverlay != null) lockOverlay.SetActive(!unlocked);

            // Locked cards are still tappable (to show the unlock hint) but cannot be chosen.
            _button.interactable = true;

            SetSelected(false);
        }

        /// <summary>Toggles the highlight frame around the currently-active character.</summary>
        public void SetSelected(bool selected)
        {
            if (selectionFrame != null) selectionFrame.SetActive(selected);
        }

        private void HandleClick()
        {
            if (_data == null) return;
            OnClicked?.Invoke(_data);
        }

        public bool IsUnlocked => _unlocked;
    }
}
