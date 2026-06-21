using UnityEngine;
using UnityEngine.EventSystems;

namespace DigitalCircus.Gameplay.InputSystem
{
    /// <summary>
    /// A lightweight on-screen analog joystick for touch devices.
    ///
    /// Prefab/scene setup (under the gameplay Canvas, bottom-left):
    ///   JoystickBackground (Image, RectTransform) ← this component lives here
    ///       └── Handle (Image, RectTransform)     ← assign to <see cref="handle"/>
    ///
    /// Placeholder art: use Unity's default UI "Knob" sprite for both the background
    /// (semi-transparent) and the handle. Replace with circus-themed art later.
    ///
    /// It uses pointer events (works for both touch and mouse), so it functions in the
    /// editor with a mouse and on-device with a finger, with no Input System asset needed.
    /// </summary>
    public class VirtualJoystick : MonoBehaviour,
        IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("References")]
        [Tooltip("The movable knob. Snaps back to center on release.")]
        [SerializeField] private RectTransform handle;

        [Header("Tuning")]
        [Tooltip("Max distance (px, in the background's local space) the handle can travel.")]
        [SerializeField] private float movementRange = 80f;
        [Tooltip("Inputs below this magnitude are treated as zero to avoid drift.")]
        [Range(0f, 0.5f)]
        [SerializeField] private float deadZone = 0.1f;

        private RectTransform _background;
        private Vector2 _input = Vector2.zero;
        private int _activePointerId = -2; // -2 = none; real pointer ids are >= -1

        /// <summary>Current stick value, each axis −1…+1, dead-zone applied.</summary>
        public Vector2 Value => _input;
        public float Horizontal => _input.x;
        public float Vertical => _input.y;

        private void Awake()
        {
            _background = transform as RectTransform;
            if (handle == null)
                Debug.LogError("[VirtualJoystick] Handle not assigned.", this);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // Claim this touch; ignore additional fingers so the jump button can be pressed
            // simultaneously without hijacking the stick.
            if (_activePointerId != -2) return;
            _activePointerId = eventData.pointerId;
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != _activePointerId) return;
            if (_background == null) return;

            // Convert the screen touch into the background's local coordinates.
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _background, eventData.position, eventData.pressEventCamera, out Vector2 local);

            // Clamp to the movement radius and normalize to −1…+1.
            Vector2 clamped = Vector2.ClampMagnitude(local, movementRange);
            _input = clamped / movementRange;

            if (_input.magnitude < deadZone) _input = Vector2.zero;

            if (handle != null) handle.anchoredPosition = clamped;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != _activePointerId) return;
            Reset();
        }

        private void OnDisable() => Reset();

        private void Reset()
        {
            _activePointerId = -2;
            _input = Vector2.zero;
            if (handle != null) handle.anchoredPosition = Vector2.zero;
        }
    }
}
