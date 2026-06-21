using System.Collections;
using UnityEngine;
using DigitalCircus.Core;
using DigitalCircus.Data;
using DigitalCircus.Gameplay.InputSystem;

namespace DigitalCircus.Characters.Abilities
{
    /// <summary>
    /// Bridges input → the correct <see cref="PlayerAbility"/> for the selected character,
    /// and enforces a cooldown. It builds the concrete ability from the character's
    /// <see cref="AbilityType"/> at startup (a tiny factory), so swapping characters swaps
    /// abilities with no other code changes.
    ///
    /// Scene setup: add next to <see cref="PlayerController2D"/> on the Player object and
    /// assign the same MobileInput reference.
    /// </summary>
    [RequireComponent(typeof(PlayerController2D))]
    public class AbilityController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private MonoBehaviour inputSourceBehaviour;

        [Header("Cooldown")]
        [Tooltip("Used only if no CharacterData is selected.")]
        [SerializeField] private float fallbackCooldown = 3f;

        public float CooldownRemaining { get; private set; }
        public float CooldownDuration { get; private set; }
        /// <summary>0…1 fraction of cooldown elapsed — wire to a HUD radial fill.</summary>
        public float CooldownNormalized =>
            CooldownDuration <= 0f ? 1f : 1f - Mathf.Clamp01(CooldownRemaining / CooldownDuration);

        private IInputSource _input;
        private PlayerController2D _player;
        private PlayerAbility _ability;
        private bool _isExecuting;

        private void Awake()
        {
            _player = GetComponent<PlayerController2D>();
            _input = inputSourceBehaviour as IInputSource;
            if (_input == null)
                Debug.LogError("[AbilityController] inputSourceBehaviour must implement IInputSource.", this);
        }

        private void Start()
        {
            BuildAbility(GameManager.Instance.SelectedCharacter);
        }

        /// <summary>Constructs the concrete ability for the given character.</summary>
        public void BuildAbility(CharacterData character)
        {
            AbilityType type = character != null ? character.Ability : AbilityType.GlitchDash;
            CooldownDuration = character != null ? character.AbilityCooldown : fallbackCooldown;

            switch (type)
            {
                case AbilityType.GlitchDash:
                    _ability = new GlitchDashAbility(_player, this);
                    break;

                // The rest are unlocked later; give a working placeholder for now.
                case AbilityType.PatchworkMend:
                    _ability = new PlaceholderAbility(_player, this, "Patchwork Mend");
                    break;
                case AbilityType.WallJumpPrank:
                    _ability = new PlaceholderAbility(_player, this, "Wall Jump & Pranks");
                    break;
                case AbilityType.RibbonGlide:
                    _ability = new PlaceholderAbility(_player, this, "Ribbon Glide");
                    break;
                case AbilityType.ErraticBlink:
                    _ability = new PlaceholderAbility(_player, this, "Erratic Blink");
                    break;
                case AbilityType.ModularSwap:
                    _ability = new PlaceholderAbility(_player, this, "Modular Swap");
                    break;

                default:
                    _ability = new GlitchDashAbility(_player, this);
                    break;
            }
        }

        private void Update()
        {
            if (CooldownRemaining > 0f)
                CooldownRemaining -= Time.deltaTime;

            if (_input == null || _ability == null) return;

            if (_input.AbilityPressedThisFrame && CanActivate())
                StartCoroutine(RunAbility());
        }

        private bool CanActivate() =>
            !_isExecuting && CooldownRemaining <= 0f && _player.ControlEnabled;

        private IEnumerator RunAbility()
        {
            _isExecuting = true;
            CooldownRemaining = CooldownDuration;

            yield return _ability.Activate();

            // Safety net: ensure physics are never left overridden if an ability errors.
            _player.SetExternalMotionOverride(false);
            _player.ResetGravity();
            _isExecuting = false;
        }
    }
}
