using UnityEngine;

namespace DigitalCircus.Characters
{
    /// <summary>
    /// Drives an <see cref="Animator"/> from <see cref="PlayerController2D"/> state so the
    /// controller stays animation-agnostic. It only sets parameters that actually exist on
    /// the assigned controller, so you can drop in a placeholder Animator (or none) without
    /// errors — wire up the real states whenever the art is ready.
    ///
    /// Expected Animator parameters (create these in the Animator Controller):
    ///   Float  "Speed"      — absolute horizontal speed (idle/run blend)
    ///   Float  "VertSpeed"  — vertical velocity (jump/fall blend)
    ///   Bool   "Grounded"   — on the ground
    ///   Trigger"Jump"       — fired the frame a jump starts
    ///
    /// Suggested placeholder: a 2-frame idle and 2-frame run made from colored squares.
    /// </summary>
    [RequireComponent(typeof(PlayerController2D))]
    public class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        private PlayerController2D _player;
        private bool _wasGrounded;

        // Cache parameter hashes + which ones exist (string lookups are slow per-frame).
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int VertSpeedHash = Animator.StringToHash("VertSpeed");
        private static readonly int GroundedHash = Animator.StringToHash("Grounded");
        private static readonly int JumpHash = Animator.StringToHash("Jump");

        private bool _hasSpeed, _hasVert, _hasGrounded, _hasJump;

        private void Awake()
        {
            _player = GetComponent<PlayerController2D>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
            CacheParameterAvailability();
        }

        private void CacheParameterAvailability()
        {
            if (animator == null || animator.runtimeAnimatorController == null) return;
            foreach (var p in animator.parameters)
            {
                if (p.nameHash == SpeedHash) _hasSpeed = true;
                else if (p.nameHash == VertSpeedHash) _hasVert = true;
                else if (p.nameHash == GroundedHash) _hasGrounded = true;
                else if (p.nameHash == JumpHash) _hasJump = true;
            }
        }

        private void Update()
        {
            if (animator == null || animator.runtimeAnimatorController == null) return;

            if (_hasSpeed) animator.SetFloat(SpeedHash, Mathf.Abs(_player.Velocity.x));
            if (_hasVert) animator.SetFloat(VertSpeedHash, _player.Velocity.y);
            if (_hasGrounded) animator.SetBool(GroundedHash, _player.IsGrounded);

            // Fire the Jump trigger on the takeoff frame (was grounded → now airborne, rising).
            if (_hasJump && _wasGrounded && !_player.IsGrounded && _player.Velocity.y > 0.1f)
                animator.SetTrigger(JumpHash);

            _wasGrounded = _player.IsGrounded;
        }
    }
}
