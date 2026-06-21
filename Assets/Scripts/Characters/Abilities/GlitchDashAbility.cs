using System.Collections;
using UnityEngine;

namespace DigitalCircus.Characters.Abilities
{
    /// <summary>
    /// Pomni's signature move: a fast horizontal teleport-dash in the facing direction.
    /// During the dash gravity is suspended and motion is overridden, giving a snappy
    /// "glitch" through gaps/hazards (full i-frame logic hooks in once combat exists).
    ///
    /// Placeholder VFX: flicker the sprite's alpha during the dash. Replace with a
    /// glitch shader / afterimage particle effect later.
    /// </summary>
    public class GlitchDashAbility : PlayerAbility
    {
        private readonly float _dashSpeed;
        private readonly float _dashDuration;
        private readonly SpriteRenderer _sprite;

        public override string DisplayName => "Glitch Dash";

        public GlitchDashAbility(PlayerController2D player, MonoBehaviour runner,
            float dashSpeed = 22f, float dashDuration = 0.18f)
            : base(player, runner)
        {
            _dashSpeed = dashSpeed;
            _dashDuration = dashDuration;
            _sprite = player.GetComponentInChildren<SpriteRenderer>();
        }

        public override IEnumerator Activate()
        {
            // Take over physics so normal movement/gravity don't fight the dash.
            Player.SetExternalMotionOverride(true);
            Player.Body.gravityScale = 0f;

            float dir = Player.FacingDirection;
            float elapsed = 0f;

            // Cheap "glitch" flicker while dashing.
            Color original = _sprite != null ? _sprite.color : Color.white;

            while (elapsed < _dashDuration)
            {
                Player.Body.linearVelocity = new Vector2(dir * _dashSpeed, 0f);

                if (_sprite != null)
                {
                    float a = Mathf.PingPong(elapsed * 30f, 1f) * 0.6f + 0.4f;
                    _sprite.color = new Color(original.r, original.g, original.b, a);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Restore visuals and physics.
            if (_sprite != null) _sprite.color = original;
            Player.Body.linearVelocity = new Vector2(Player.Body.linearVelocity.x * 0.5f, Player.Body.linearVelocity.y);
            Player.ResetGravity();
            Player.SetExternalMotionOverride(false);
        }
    }
}
