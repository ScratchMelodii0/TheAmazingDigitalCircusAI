using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using DigitalCircus.Data;

namespace DigitalCircus.UI
{
    /// <summary>
    /// One episode entry in the Hub's chapter-select list.
    ///
    /// Prefab (Assets/Prefabs/EpisodeCard.prefab):
    ///   Root: Button (this script)
    ///     ├── Thumbnail   (Image)
    ///     ├── NumberLabel (TMP_Text)  e.g. "1"
    ///     ├── TitleLabel  (TMP_Text)  e.g. "Pilot"
    ///     ├── LockOverlay (GameObject)  shown when locked
    ///     └── SelectionFrame (GameObject)  shown when selected
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class EpisodeCardUI : MonoBehaviour
    {
        [SerializeField] private Image thumbnail;
        [SerializeField] private Image background;
        [SerializeField] private TMP_Text numberLabel;
        [SerializeField] private TMP_Text titleLabel;
        [SerializeField] private GameObject lockOverlay;
        [SerializeField] private GameObject selectionFrame;

        private Button _button;
        private EpisodeData _data;

        public UnityAction<EpisodeData> OnClicked;
        public EpisodeData Data => _data;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(() => OnClicked?.Invoke(_data));
        }

        public void Bind(EpisodeData data, bool unlocked)
        {
            _data = data;
            if (data == null) { gameObject.SetActive(false); return; }
            gameObject.SetActive(true);

            if (numberLabel != null) numberLabel.text = data.EpisodeNumber.ToString();
            if (titleLabel != null) titleLabel.text = unlocked ? data.Title : "Locked";

            if (thumbnail != null)
            {
                thumbnail.sprite = data.Thumbnail;
                thumbnail.color = unlocked ? Color.white : new Color(0.2f, 0.2f, 0.2f, 1f);
            }
            if (background != null)
                background.color = unlocked ? data.ThemeColor : Color.gray;

            if (lockOverlay != null) lockOverlay.SetActive(!unlocked);
            SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            if (selectionFrame != null) selectionFrame.SetActive(selected);
        }
    }
}
