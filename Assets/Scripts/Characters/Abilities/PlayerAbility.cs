using System.Collections;
using UnityEngine;

namespace DigitalCircus.Characters.Abilities
{
    /// <summary>
    /// Base class for a character's signature ability. Abilities are plain C# objects
    /// (not MonoBehaviours) driven by <see cref="AbilityController"/>, which owns the
    /// cooldown and the coroutine runner. Each concrete ability only implements its
    /// effect in <see cref="Activate"/> — keeping the moves small and self-contained.
    /// </summary>
    public abstract class PlayerAbility
    {
        protected readonly PlayerController2D Player;
        protected readonly MonoBehaviour Runner; // used to start coroutines

        protected PlayerAbility(PlayerController2D player, MonoBehaviour runner)
        {
            Player = player;
            Runner = runner;
        }

        /// <summary>Human-readable name for HUD/debug.</summary>
        public abstract string DisplayName { get; }

        /// <summary>
        /// Runs the ability. Implemented as a coroutine so timed effects (dashes,
        /// glides, i-frames) are easy to express. Return immediately for instant moves.
        /// </summary>
        public abstract IEnumerator Activate();
    }
}
