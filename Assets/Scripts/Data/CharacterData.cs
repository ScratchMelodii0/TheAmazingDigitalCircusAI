using UnityEngine;

namespace DigitalCircus.Data
{
    /// <summary>
    /// Data-driven definition of a single playable cast member.
    /// Designers create one asset per character via:
    ///   Assets ▸ Create ▸ Digital Circus ▸ Character Data
    ///
    /// Using a ScriptableObject keeps tuning values (speed, jump, sanity) out of
    /// code so balancing the game never requires a recompile, and the same asset
    /// can be referenced by the selection menu, the player controller and the HUD.
    /// </summary>
    [CreateAssetMenu(
        fileName = "NewCharacter",
        menuName = "Digital Circus/Character Data",
        order = 0)]
    public class CharacterData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Stable unique id used by the save system. NEVER rename once shipped.")]
        [SerializeField] private string characterId = "pomni";
        [Tooltip("Display name shown in menus and dialogue.")]
        [SerializeField] private string displayName = "Pomni";
        [TextArea(2, 4)]
        [SerializeField] private string description = "A nervous jester desperate to find the exit.";

        [Header("Presentation")]
        [Tooltip("Portrait shown on the selection card. Placeholder: a colored square sprite.")]
        [SerializeField] private Sprite portrait;
        [Tooltip("In-game body sprite. Placeholder: a 1x2 capsule/rectangle sprite.")]
        [SerializeField] private Sprite bodySprite;
        [Tooltip("Theme color used to tint UI cards and particle effects.")]
        [SerializeField] private Color themeColor = Color.white;

        [Header("Movement Tuning")]
        [Min(0f)]
        [SerializeField] private float moveSpeed = 7f;
        [Min(0f)]
        [SerializeField] private float jumpForce = 14f;
        [Tooltip("How fast this character loses sanity passively (per second). Gangle/Kinger should be higher.")]
        [Min(0f)]
        [SerializeField] private float sanityDrainRate = 1f;

        [Header("Ability")]
        [SerializeField] private AbilityType ability = AbilityType.GlitchDash;
        [Tooltip("Cooldown in seconds before the ability can be used again.")]
        [Min(0f)]
        [SerializeField] private float abilityCooldown = 3f;

        [Header("Unlock Rules")]
        [Tooltip("If true the character is available from the very first launch (Pomni).")]
        [SerializeField] private bool unlockedByDefault = true;
        [Tooltip("Hint shown on the card while still locked.")]
        [SerializeField] private string unlockHint = "Complete the Pilot to unlock.";

        // ---- Read-only public accessors (data assets are never mutated at runtime) ----
        public string CharacterId => characterId;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Portrait => portrait;
        public Sprite BodySprite => bodySprite;
        public Color ThemeColor => themeColor;
        public float MoveSpeed => moveSpeed;
        public float JumpForce => jumpForce;
        public float SanityDrainRate => sanityDrainRate;
        public AbilityType Ability => ability;
        public float AbilityCooldown => abilityCooldown;
        public bool UnlockedByDefault => unlockedByDefault;
        public string UnlockHint => unlockHint;

#if UNITY_EDITOR
        /// <summary>
        /// Editor-only guard rails so a misconfigured asset is caught early.
        /// </summary>
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(characterId))
                Debug.LogWarning($"[CharacterData] '{name}' has an empty characterId — the save system needs a stable id.", this);
        }
#endif
    }
}
