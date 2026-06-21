using System.Collections.Generic;
using UnityEngine;

namespace DigitalCircus.Data
{
    /// <summary>
    /// Ordered list of every <see cref="EpisodeData"/> (Pilot first, The Last Act last).
    /// Create via Assets ▸ Create ▸ Digital Circus ▸ Episode Database and place it under
    /// Assets/Resources/Data/ so the <c>GameManager</c> can load it without a scene ref.
    /// </summary>
    [CreateAssetMenu(
        fileName = "EpisodeDatabase",
        menuName = "Digital Circus/Episode Database",
        order = 3)]
    public class EpisodeDatabase : ScriptableObject
    {
        [Tooltip("All episodes in play order.")]
        [SerializeField] private List<EpisodeData> episodes = new List<EpisodeData>();

        public const string ResourcePath = "Data/EpisodeDatabase";

        public IReadOnlyList<EpisodeData> Episodes => episodes;
        public int Count => episodes.Count;

        public EpisodeData GetByIndex(int index)
        {
            if (index < 0 || index >= episodes.Count)
            {
                Debug.LogError($"[EpisodeDatabase] Index {index} out of range (count {episodes.Count}).", this);
                return null;
            }
            return episodes[index];
        }

        public EpisodeData GetById(string episodeId)
        {
            if (string.IsNullOrEmpty(episodeId)) return null;
            for (int i = 0; i < episodes.Count; i++)
                if (episodes[i] != null && episodes[i].EpisodeId == episodeId)
                    return episodes[i];
            return null;
        }

        public static EpisodeDatabase Load()
        {
            var db = Resources.Load<EpisodeDatabase>(ResourcePath);
            if (db == null)
            {
                Debug.LogError(
                    $"[EpisodeDatabase] Could not load from 'Resources/{ResourcePath}'. " +
                    "Create the asset and place it under Assets/Resources/Data/.");
            }
            return db;
        }
    }
}
