using System;
using Control.Note;
using UnityEngine;
using Utility;
using Utility.DependencyInjection;

namespace Game
{
    public sealed class NoteInstaller : MonoBehaviour, IInstaller
    {
        [Inject] private NoteBehaviour _notePrefab;
        [Inject] private NoteManager _manager;
        [Inject] private PlayableSettings _settings;
        [Inject] private ResponsiveLayout _layout;

        public void AwakeInitialize()
        {
            // Validate song, chart, and timing before building the note system.
            if (_notePrefab == null || _manager == null || _settings == null || _layout == null ||
                _settings.Chart == null || _settings.Song == null ||
                _settings.TravelSeconds <= 0f || _settings.HitWindowSeconds < 0f)
            {
                Debug.LogError("NoteInstaller needs a note manager, prefab, chart, song and valid timing settings.", this);
                return;
            }

            // The prefab needs a Kinematic Rigidbody2D and trigger collider for 2D hit events.
            if (!_notePrefab.TryGetComponent<Rigidbody2D>(out var rb) ||
                rb.bodyType != RigidbodyType2D.Kinematic ||
                !_notePrefab.TryGetComponent<CircleCollider2D>(out var col) || !col.isTrigger)
            {
                Debug.LogError("Note prefab needs a Kinematic Rigidbody2D and trigger CircleCollider2D.", this);
                return;
            }

            try
            {
                // Parse once during setup and pass the sorted NoteEvent array to the manager.
                var chart = NoteChart.Parse(_settings.Chart.text);
                _layout.Refresh();
                // Keep the music source with the injected NoteManager, even if the installer moves.
                var music = _manager.gameObject.GetOrAdd<AudioSource>();
                _manager.Initialize(chart, _notePrefab, _settings, _layout, music);
            }
            catch (FormatException ex)
            {
                Debug.LogError($"Invalid note chart: {ex.Message}", this);
            }
        }

        public void StartInitialize() { }

        private void Awake() => AwakeInitialize();
        private void Start() => StartInitialize();
    }
}
