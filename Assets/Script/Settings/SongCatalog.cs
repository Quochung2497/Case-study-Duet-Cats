using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    // Playback setup only; the manager does not need the definition's ID or JSON chart.
    public interface ISongPlaybackSettings
    {
        AudioClip Audio { get; }
        bool PlayOnAwake { get; }
        bool Loop { get; }
        float SpatialBlend { get; }
    }

    [Serializable]
    public struct SongDefinition : ISongPlaybackSettings
    {
        [Tooltip("Unique stable ID used by scene song choices.")]
        [SerializeField] private string id;
        [SerializeField] private AudioClip audio;
        [SerializeField] private TextAsset chart;

        [Header("Audio playback")]
        [Tooltip("AudioSource lifecycle setting. The playable session still starts through StartSong.")]
        [SerializeField] private bool playOnAwake;
        [Tooltip("Loops the AudioSource only. Chart completion still ends the playable run.")]
        [SerializeField] private bool loop;
        [Range(0f, 1f)] [SerializeField] private float spatialBlend;

        public string Id => id;
        public AudioClip Audio => audio;
        public TextAsset Chart => chart;
        public bool PlayOnAwake => playOnAwake;
        public bool Loop => loop;
        public float SpatialBlend => spatialBlend;

        public SongDefinition(string id, AudioClip audio, TextAsset chart,
            bool playOnAwake = false, bool loop = false, float spatialBlend = 0f)
        {
            this.id = id;
            this.audio = audio;
            this.chart = chart;
            this.playOnAwake = playOnAwake;
            this.loop = loop;
            this.spatialBlend = spatialBlend;
        }
    }

    [CreateAssetMenu(fileName = "SongCatalog", menuName = "Playable/Song Catalog")]
    public class SongCatalog : ScriptableObject
    {
        [SerializeField] private List<SongDefinition> songs = new();

        public IReadOnlyList<SongDefinition> Songs => songs;

        public SongDefinition Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new InvalidOperationException("The selected song needs an ID from the catalog.");

            SongDefinition? selected = null;
            if (songs != null)
                foreach (var song in songs)
                {
                    if (song.Id != id) continue;
                    if (selected.HasValue)
                        throw new InvalidOperationException($"Song catalog contains duplicate ID '{id}'.");

                    selected = song;
                }

            return selected ?? throw new InvalidOperationException($"Song catalog has no entry with ID '{id}'.");
        }
    }
}
