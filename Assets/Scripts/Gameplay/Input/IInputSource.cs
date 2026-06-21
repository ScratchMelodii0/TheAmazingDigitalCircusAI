using UnityEngine;

namespace DigitalCircus.Gameplay.InputSystem
{
    /// <summary>
    /// Abstraction over "where movement intent comes from" so the player controller
    /// never cares whether it is driven by an on-screen joystick, a keyboard (editor),
    /// or a future gamepad. The controller depends only on this interface, which keeps
    /// it testable and lets us swap input schemes without touching gameplay code.
    /// </summary>
    public interface IInputSource
    {
        /// <summary>Horizontal move axis, −1 (left) … +1 (right).</summary>
        float Horizontal { get; }

        /// <summary>Vertical aim/look axis, −1 (down) … +1 (up). Used for ladders/aim later.</summary>
        float Vertical { get; }

        /// <summary>True only on the frame the jump button went down (for jump buffering).</summary>
        bool JumpPressedThisFrame { get; }

        /// <summary>True while the jump button is held (for variable jump height).</summary>
        bool JumpHeld { get; }

        /// <summary>True only on the frame the ability button went down.</summary>
        bool AbilityPressedThisFrame { get; }
    }
}
