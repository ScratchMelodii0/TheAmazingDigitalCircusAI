using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DigitalCircus.Core;

namespace DigitalCircus.UI
{
    /// <summary>
    /// Binds a fill bar (and optional label) to the <see cref="GameManager"/> sanity value.
    /// Lerps the fill for a smooth drain and shifts the color from calm → alarming as the
    /// player approaches "abstraction" — the show's core dread, surfaced as a HUD element.
    ///
    /// Scene setup: an Image with Image Type = Filled (Horizontal) for <see cref="fillImage"/>,
    /// plus an optional TMP_Text. Works in both the Hub (passive display) and levels
    /// (where sanity actively drains).
    /// </summary>
    public class SanityMeterUI : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Image set to Type=Filled. Its fillAmount is driven 0…1.")]
        [SerializeField] private Image fillImage;
        [SerializeField] private TMP_Text valueLabel;

        [Header("Appearance")]
        [SerializeField] private Color healthyColor = new Color(0.4f, 0.85f, 0.5f);
        [SerializeField] private Color warningColor = new Color(0.95f, 0.8f, 0.3f);
        [SerializeField] private Color dangerColor = new Color(0.9f, 0.25f, 0.3f);
        [Tooltip("How quickly the bar catches up to the true value.")]
        [SerializeField] private float lerpSpeed = 4f;

        private float _displayedFraction = 1f;
        private float _targetFraction = 1f;

        private void OnEnable()
        {
            var gm = GameManager.Instance;
            gm.OnSanityChanged += HandleSanityChanged;
            // Initialize immediately so the bar is correct on the first frame.
            HandleSanityChanged(gm.Sanity, GameManager.MaxSanity);
            _displayedFraction = _targetFraction;
        }

        private void OnDisable()
        {
            // Guard: Instance may be torn down during app quit.
            if (GameManager.Instance != null)
                GameManager.Instance.OnSanityChanged -= HandleSanityChanged;
        }

        private void HandleSanityChanged(float current, float max)
        {
            _targetFraction = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            if (valueLabel != null)
                valueLabel.text = $"{Mathf.RoundToInt(current)}%";
        }

        private void Update()
        {
            if (fillImage == null) return;

            _displayedFraction = Mathf.MoveTowards(
                _displayedFraction, _targetFraction, lerpSpeed * Time.unscaledDeltaTime);
            fillImage.fillAmount = _displayedFraction;

            // Two-stage gradient: danger→warning in the lower half, warning→healthy above.
            fillImage.color = _displayedFraction < 0.5f
                ? Color.Lerp(dangerColor, warningColor, _displayedFraction * 2f)
                : Color.Lerp(warningColor, healthyColor, (_displayedFraction - 0.5f) * 2f);
        }
    }
}
