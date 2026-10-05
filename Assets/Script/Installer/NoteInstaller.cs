using System;
using Control.Note;
using UnityEngine;
using Utility;
using Utility.DependencyInjection;

namespace Game
{
    public sealed class NoteInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private NoteBehaviour notePrefab;

        [Inject] private PlayableSettings _settings;
        [Inject] private ResponsiveLayout _layout;

        public void AwakeInitialize()
        {
            if (notePrefab == null || _settings == null || _layout == null ||
                _settings.Chart == null || _settings.Song == null ||
                _settings.TravelSeconds <= 0f || _settings.HitWindowSeconds < 0f)
            {
                Debug.LogError("NoteInstaller needs a note prefab, chart, song and valid timing settings.", this);
                return;
            }

            if (!notePrefab.TryGetComponent<Rigidbody2D>(out var rb) ||
                rb.bodyType != RigidbodyType2D.Kinematic ||
                !notePrefab.TryGetComponent<CircleCollider2D>(out var col) || !col.isTrigger)
            {
                Debug.LogError("Note prefab needs a Kinematic Rigidbody2D and trigger CircleCollider2D.", this);
                return;
            }

            try
            {
                var chart = NoteChart.Parse(_settings.Chart.text);
                _layout.Refresh();
                var manager = gameObject.GetOrAdd<NoteManager>();
                var music = gameObject.GetOrAdd<AudioSource>();
                manager.Initialize(chart, notePrefab, _settings, _layout, music);
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
