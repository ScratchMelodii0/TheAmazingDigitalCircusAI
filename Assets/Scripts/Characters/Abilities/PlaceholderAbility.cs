using System.Collections;
using UnityEngine;

namespace DigitalCircus.Characters.Abilities
{
    /// <summary>
    /// Temporary stand-in for abilities not yet implemented (Ragatha/Jax/Gangle/Kinger/
    /// Zooble are unlocked in later episodes). It performs a small, safe vertical "hop"
    /// so the ability button does something during testing, and logs which move is pending.
    /// Replace each with a dedicated <see cref="PlayerAbility"/> in later steps.
    /// </summary>
    public class PlaceholderAbility : PlayerAbility
    {
        public override string DisplayName { get; }

        public PlaceholderAbility(PlayerController2D player, MonoBehaviour runner, string name)
            : base(player, runner)
        {
            DisplayName = name;
        }

        public override IEnumerator Activate()
        {
            Debug.Log($"[Ability] '{DisplayName}' not implemented yet — placeholder hop.");
            // Tiny upward nudge so the press has visible feedback.
            Player.Body.linearVelocity = new Vector2(Player.Body.linearVelocity.x, 6f);
            yield break;
        }
    }
}
