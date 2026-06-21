using UnityEngine;
using DigitalCircus.Core;
using DigitalCircus.Data;
using DigitalCircus.Gameplay.InputSystem;

namespace DigitalCircus.Characters
{
    /// <summary>
    /// Rigidbody2D-based side-scrolling character controller with the feel staples that
    /// make a platformer playable on a phone: coyote time, jump buffering, and variable
    /// jump height. Movement tuning (speed, jump force) is pulled from the player's
    /// selected <see cref="CharacterData"/> so every cast member handles differently
    /// with zero per-character code.
    ///
    /// Required components (auto-required): Rigidbody2D + a Collider2D.
    /// Scene setup:
    ///   Player (this script, Rigidbody2D [gravity 3-4, freeze rotation Z], CapsuleCollider2D)
    ///     ├── GroundCheck (empty Transform at the feet) → assign to groundCheck
    ///     └── Sprite (SpriteRenderer)                   → assign to spriteRenderer
    ///   Assign the GroundLayer mask and a MobileInput reference.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class PlayerController2D : MonoBehaviour
    {
        [Header("Input")]
        [Tooltip("Any component implementing IInputSource (e.g. MobileInput). Required.")]
        [SerializeField] private MonoBehaviour inputSourceBehaviour;

        [Header("References")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [Tooltip("Empty child at the character's feet used for the ground overlap test.")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private LayerMask groundLayer;

        [Header("Fallback Movement (used if no CharacterData is selected)")]
        [SerializeField] private float fallbackMoveSpeed = 7f;
        [SerializeField] private float fallbackJumpForce = 14f;

        [Header("Ground Check")]
        [SerializeField] private float groundCheckRadius = 0.18f;

        [Header("Jump Feel")]
        [Tooltip("Grace period after leaving a ledge during which you can still jump.")]
        [SerializeField] private float coyoteTime = 0.10f;
        [Tooltip("How long a jump press is remembered before landing.")]
        [SerializeField] private float jumpBufferTime = 0.10f;
        [Tooltip("Extra gravity while falling, for a snappier arc.")]
        [SerializeField] private float fallGravityMultiplier = 1.8f;
        [Tooltip("Extra gravity when rising but jump was released (variable height).")]
        [SerializeField] private float lowJumpMultiplier = 2.5f;
        [Tooltip("Horizontal acceleration smoothing. Lower = snappier.")]
        [Range(0f, 0.3f)]
        [SerializeField] private float moveSmoothing = 0.05f;

        // ---- Runtime state (exposed read-only for the animator & abilities) -------
        public bool IsGrounded { get; private set; }
        public int FacingDirection { get; private set; } = 1; // 1 = right, −1 = left
        public Vector2 Velocity => _rb.linearVelocity;
        public Rigidbody2D Body => _rb;
        public bool ControlEnabled { get; set; } = true;

        private IInputSource _input;
        private Rigidbody2D _rb;
        private CharacterData _character;

        private float _moveSpeed;
        private float _jumpForce;
        private float _baseGravityScale;

        private float _coyoteTimer;
        private float _jumpBufferTimer;
        private float _velXSmoothRef;

        // When an ability (e.g. dash) takes over motion, normal movement pauses.
        private bool _externalMotionOverride;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _input = inputSourceBehaviour as IInputSource;

            if (_input == null)
                Debug.LogError("[PlayerController2D] inputSourceBehaviour must implement IInputSource.", this);
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (groundCheck == null)
                Debug.LogWarning("[PlayerController2D] No groundCheck assigned — using collider bounds.", this);

            _baseGravityScale = _rb.gravityScale;
            _rb.freezeRotation = true;
        }

        private void Start()
        {
            ApplyCharacter(GameManager.Instance.SelectedCharacter);
        }

        /// <summary>Applies tuning + body sprite from the chosen character (or fallbacks).</summary>
        public void ApplyCharacter(CharacterData character)
        {
            _character = character;
            _moveSpeed = character != null ? character.MoveSpeed : fallbackMoveSpeed;
            _jumpForce = character != null ? character.JumpForce : fallbackJumpForce;

            if (character != null && character.BodySprite != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = character.BodySprite;
                spriteRenderer.color = Color.white;
            }
        }

        private void Update()
        {
            if (_input == null) return;

            // Buffer the jump press so a slightly-early tap still triggers on landing.
            if (ControlEnabled && _input.JumpPressedThisFrame)
                _jumpBufferTimer = jumpBufferTime;
            else
                _jumpBufferTimer -= Time.deltaTime;
        }

        private void FixedUpdate()
        {
            if (_input == null) return;

            UpdateGrounded();

            if (!_externalMotionOverride)
            {
                ApplyHorizontalMovement();
                HandleJump();
                ApplyBetterGravity();
            }
        }

        private void UpdateGrounded()
        {
            bool wasGrounded = IsGrounded;

            if (groundCheck != null)
            {
                IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
            }
            else
            {
                // Fallback: a thin box cast just under the collider.
                var col = GetComponent<Collider2D>();
                Vector2 origin = new Vector2(col.bounds.center.x, col.bounds.min.y);
                IsGrounded = Physics2D.OverlapBox(origin, new Vector2(col.bounds.size.x * 0.9f, 0.1f), 0f, groundLayer);
            }

            // Refresh coyote timer the moment we are on the ground.
            if (IsGrounded) _coyoteTimer = coyoteTime;
            else _coyoteTimer -= Time.fixedDeltaTime;
        }

        private void ApplyHorizontalMovement()
        {
            if (!ControlEnabled)
            {
                // Decelerate to a stop when control is locked (cutscenes/dialogue).
                float stopped = Mathf.SmoothDamp(_rb.linearVelocity.x, 0f, ref _velXSmoothRef, moveSmoothing);
                _rb.linearVelocity = new Vector2(stopped, _rb.linearVelocity.y);
                return;
            }

            float targetX = _input.Horizontal * _moveSpeed;
            float smoothedX = Mathf.SmoothDamp(_rb.linearVelocity.x, targetX, ref _velXSmoothRef, moveSmoothing);
            _rb.linearVelocity = new Vector2(smoothedX, _rb.linearVelocity.y);

            // Flip the sprite to face travel direction (ignore tiny stick noise).
            if (_input.Horizontal > 0.05f) SetFacing(1);
            else if (_input.Horizontal < -0.05f) SetFacing(-1);
        }

        private void HandleJump()
        {
            bool canJump = _coyoteTimer > 0f && _jumpBufferTimer > 0f;
            if (!canJump) return;

            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _jumpForce);
            _jumpBufferTimer = 0f;
            _coyoteTimer = 0f;
        }

        private void ApplyBetterGravity()
        {
            // Falling → heavier; rising with jump released → cut the jump short.
            if (_rb.linearVelocity.y < 0f)
            {
                _rb.gravityScale = _baseGravityScale * fallGravityMultiplier;
            }
            else if (_rb.linearVelocity.y > 0f && !_input.JumpHeld)
            {
                _rb.gravityScale = _baseGravityScale * lowJumpMultiplier;
            }
            else
            {
                _rb.gravityScale = _baseGravityScale;
            }
        }

        private void SetFacing(int dir)
        {
            if (dir == FacingDirection) return;
            FacingDirection = dir;
            if (spriteRenderer != null)
                spriteRenderer.flipX = dir < 0;
        }

        // ---- API for the ability system ------------------------------------------

        /// <summary>Lets an ability (e.g. Glitch Dash) take over physics temporarily.</summary>
        public void SetExternalMotionOverride(bool active) => _externalMotionOverride = active;

        /// <summary>Restores the character's default gravity scale (abilities use this).</summary>
        public void ResetGravity() => _rb.gravityScale = _baseGravityScale;

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (groundCheck == null) return;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
#endif
    }
}
