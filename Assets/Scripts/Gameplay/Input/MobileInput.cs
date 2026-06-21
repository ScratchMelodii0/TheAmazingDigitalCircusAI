using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace DigitalCircus.Gameplay.InputSystem
{
    /// <summary>
    /// Aggregates the on-screen <see cref="VirtualJoystick"/> and <see cref="TouchButton"/>s
    /// into a single <see cref="IInputSource"/> the player controller consumes.
    ///
    /// In the editor it also reads the keyboard (A/D or arrows, Space = jump,
    /// Left-Shift/J = ability) so you can test without dragging a virtual stick around.
    /// The keyboard path is compiled against the new Input System when present and falls
    /// back to the legacy Input Manager otherwise, so it works in any project config.
    ///
    /// Scene setup: place this on a "GameplayInput" object on the HUD Canvas and assign
    /// the joystick + two buttons. The player controller finds it via the serialized ref
    /// you give it (it is a plain MonoBehaviour, not a singleton, so multiple control
    /// schemes could coexist for local co-op later).
    /// </summary>
    public class MobileInput : MonoBehaviour, IInputSource
    {
        [Header("On-Screen Controls")]
        [SerializeField] private VirtualJoystick joystick;
        [SerializeField] private TouchButton jumpButton;
        [SerializeField] private TouchButton abilityButton;

        [Header("Editor / Desktop Testing")]
        [Tooltip("Allow keyboard input in addition to the on-screen controls.")]
        [SerializeField] private bool enableKeyboardFallback = true;

        public float Horizontal
        {
            get
            {
                float h = joystick != null ? joystick.Horizontal : 0f;
                if (enableKeyboardFallback) h += ReadKeyboardHorizontal();
                return Mathf.Clamp(h, -1f, 1f);
            }
        }

        public float Vertical
        {
            get
            {
                float v = joystick != null ? joystick.Vertical : 0f;
                if (enableKeyboardFallback) v += ReadKeyboardVertical();
                return Mathf.Clamp(v, -1f, 1f);
            }
        }

        public bool JumpPressedThisFrame =>
            (jumpButton != null && jumpButton.WasPressedThisFrame) ||
            (enableKeyboardFallback && KeyboardJumpDown());

        public bool JumpHeld =>
            (jumpButton != null && jumpButton.IsHeld) ||
            (enableKeyboardFallback && KeyboardJumpHeld());

        public bool AbilityPressedThisFrame =>
            (abilityButton != null && abilityButton.WasPressedThisFrame) ||
            (enableKeyboardFallback && KeyboardAbilityDown());

        // ---- Keyboard helpers (cross-config) -------------------------------------

        private static float ReadKeyboardHorizontal()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb == null) return 0f;
            float h = 0f;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) h -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) h += 1f;
            return h;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return UnityEngine.Input.GetAxisRaw("Horizontal");
#else
            return 0f;
#endif
        }

        private static float ReadKeyboardVertical()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb == null) return 0f;
            float v = 0f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) v -= 1f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) v += 1f;
            return v;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return UnityEngine.Input.GetAxisRaw("Vertical");
#else
            return 0f;
#endif
        }

        private static bool KeyboardJumpDown()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return UnityEngine.Input.GetKeyDown(KeyCode.Space);
#else
            return false;
#endif
        }

        private static bool KeyboardJumpHeld()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return UnityEngine.Input.GetKey(KeyCode.Space);
#else
            return false;
#endif
        }

        private static bool KeyboardAbilityDown()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            return kb != null && (kb.leftShiftKey.wasPressedThisFrame || kb.jKey.wasPressedThisFrame);
#elif ENABLE_LEGACY_INPUT_MANAGER
            return UnityEngine.Input.GetKeyDown(KeyCode.LeftShift) || UnityEngine.Input.GetKeyDown(KeyCode.J);
#else
            return false;
#endif
        }
    }
}
