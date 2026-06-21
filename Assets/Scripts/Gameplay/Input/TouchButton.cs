using UnityEngine;
using UnityEngine.EventSystems;

namespace DigitalCircus.Gameplay.InputSystem
{
    /// <summary>
    /// A multi-touch-friendly on-screen button that reports both "held" and
    /// "pressed this frame" states — the standard Unity <c>Button</c> only fires a
    /// click on release, which is wrong for an action game (you want jump on press).
    ///
    /// Scene setup: put this on an Image inside the gameplay Canvas (e.g. a Jump and
    /// an Ability button, bottom-right). Placeholder art: default UI sprite, tinted.
    /// </summary>
    public class TouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        /// <summary>True while a finger/mouse is holding the button down.</summary>
        public bool IsHeld { get; private set; }

        // Frame stamps let us answer "this frame?" without a manual reset step that
        // could be missed depending on script execution order.
        private int _pressedFrame = -1;
        private int _releasedFrame = -1;

        /// <summary>True only on the frame the button was first pressed.</summary>
        public bool WasPressedThisFrame => _pressedFrame == Time.frameCount;

        /// <summary>True only on the frame the button was released.</summary>
        public bool WasReleasedThisFrame => _releasedFrame == Time.frameCount;

        public void OnPointerDown(PointerEventData eventData)
        {
            IsHeld = true;
            _pressedFrame = Time.frameCount;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            IsHeld = false;
            _releasedFrame = Time.frameCount;
        }

        // Safety: if the object is disabled mid-press (e.g. pause), don't get stuck "held".
        private void OnDisable() => IsHeld = false;
    }
}
