using System;
using Control.Note;
using UnityEngine;
using Utility.DependencyInjection;

namespace Game
{
    public sealed class NoteInstaller : MonoBehaviour, IInstaller
    {
        [Inject] private NoteBehaviour _notePrefab;
        [Inject] private INoteManager _manager;
        [Inject] private INoteSettings _settings;
        [Inject] private SongDefinition _song;
        [Inject] private IResponsiveLayout _layout;
        [Inject] private IScoreManager _score;
        [Inject] private Transform _noteParent;
        private NotePool _pool;

        public void AwakeInitialize()
        {
            // Validate song, chart, and timing before building the note system.
            if (_noteParent == null || _notePrefab == null || _manager == null ||
                _settings == null || _layout == null || _score == null ||
                _song.Chart == null || _song.Audio == null ||
                _settings.TravelSeconds <= 0f || _settings.HitWindowSeconds < 0f ||
                string.IsNullOrWhiteSpace(_settings.CatLayerName) ||
                LayerMask.NameToLayer(_settings.CatLayerName) < 0)
            {
                Debug.LogError("NoteInstaller needs a note parent, manager, prefab, chart, song, valid timing and a cat layer.", this);
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
                // Parse once and build the timeline before wiring the manager.
                var chart = NoteChart.Parse(_song.Chart.text);
                INoteTimeline timeline = new NoteTimeline(chart, _settings.TravelSeconds);
                _layout.RecalculateLayout();
                _pool = new NotePool(_notePrefab, _noteParent, _score, _settings.CatLayerName);
                _manager.Initialize(timeline, _pool, _song, _settings, _layout);
            }
            catch (FormatException ex)
            {
                Debug.LogError($"Invalid note chart: {ex.Message}", this);
            }
        }

        public void StartInitialize() { }

        private void Awake() => AwakeInitialize();
        private void Start() => StartInitialize();
        private void OnDestroy() => _pool?.GetPool().Clear();
    }
}
