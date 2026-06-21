namespace DigitalCircus.Data
{
    /// <summary>
    /// Defines the unique signature ability each playable cast member has.
    /// The platformer controller (added in a later step) reads this enum to
    /// decide which special move to execute when the player taps the ability button.
    /// Keep this in sync with the show's cast so designers can pick from the dropdown
    /// directly inside a <see cref="CharacterData"/> ScriptableObject.
    /// </summary>
    public enum AbilityType
    {
        /// <summary>Pomni — short-range teleport/dash that flickers through hazards.</summary>
        GlitchDash = 0,

        /// <summary>Ragatha — heals a small amount of sanity and can stitch broken platforms.</summary>
        PatchworkMend = 1,

        /// <summary>Jax — wall jump plus placeable prank traps.</summary>
        WallJumpPrank = 2,

        /// <summary>Gangle — fragile float/glide; takes more stress but reaches high places.</summary>
        RibbonGlide = 3,

        /// <summary>Kinger — erratic random teleport (high risk, high reward).</summary>
        ErraticBlink = 4,

        /// <summary>Zooble — swap modular body parts to gain a temporary buff.</summary>
        ModularSwap = 5
    }
}
