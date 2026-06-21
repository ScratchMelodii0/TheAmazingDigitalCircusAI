using System;
using System.Collections.Generic;

namespace DigitalCircus.Core
{
    /// <summary>
    /// Plain serializable container for everything that persists between sessions.
    /// Kept deliberately simple (fields, no logic) so Unity's <c>JsonUtility</c> can
    /// round-trip it. Versioned so future migrations can detect old save shapes.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        /// <summary>Bump when the schema changes in a breaking way.</summary>
        public int saveVersion = 1;

        /// <summary>Id of the character last selected (defaults to Pomni).</summary>
        public string selectedCharacterId = "pomni";

        /// <summary>Stable ids of characters the player has unlocked.</summary>
        public List<string> unlockedCharacters = new List<string>();

        /// <summary>Highest episode index the player may enter (0-based; 0 = Pilot only).</summary>
        public int highestUnlockedEpisode = 0;

        /// <summary>Persisted "abstraction" / sanity meter, 0–100. 100 = perfectly fine.</summary>
        public float sanity = 100f;

        /// <summary>Free-form key/value flags for dialogue choices and endings.</summary>
        public List<StoryFlag> storyFlags = new List<StoryFlag>();

        /// <summary>Unix timestamp (seconds) of the last save, for "continue" displays.</summary>
        public long lastSavedUnix;

        [Serializable]
        public struct StoryFlag
        {
            public string key;
            public int value;
        }
    }
}
