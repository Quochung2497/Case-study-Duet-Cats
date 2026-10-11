using System;
using System.Collections.Generic;
using Control.Note;
using UnityEngine;
using Utility;

namespace Game
{
    [DisallowMultipleComponent]
    public class NoteManager : MonoBehaviour, INoteManager
    {
        private readonly List<NoteBehaviour> _active = new();
        private NoteEvent[] _chart;
        private NoteTimeline _timeline;
        private NotePool _pool;
        private INoteSettings _settings;
        private ResponsiveLayout _layout;
        private AudioSource _music;
        private double _songStartDsp;
        private bool _playing;

        public event Action NoteMissed;
        public event Action SongFinished;

        public void Initialize(NoteEvent[] chart, NoteBehaviour prefab,
            INoteSettings settings, ResponsiveLayout layout, IScoreManager score)
        {
            // The installer provides dependencies once; the pool creates notes when needed.
            _chart = chart;
            _pool = new NotePool(prefab, transform, score);
            _settings = settings;
            _layout = layout;
            _music = gameObject.GetOrAdd<AudioSource>();
        }

        public bool StartSong()
        {
            if (_chart == null || _settings == null || _settings.Song == null || _music == null)
                return false;

            StopSong();
            // Create a fresh timeline for each run so its note cursor starts at zero.
            _timeline = new NoteTimeline(_chart, _settings.TravelSeconds);
            _music.clip = _settings.Song;
            _music.playOnAwake = false;
            _music.loop = false;
            _music.spatialBlend = 0f;
            _songStartDsp = AudioSettings.dspTime + 0.1d;
            // Use the DSP clock for both audio and note positions to avoid frame-rate drift.
            _music.PlayScheduled(_songStartDsp);
            _playing = true;
            return true;
        }

        public void StopSong()
        {
            // Return all active notes to the pool when the run ends.
            _playing = false;
            if (_music != null) _music.Stop();
            foreach (var note in _active)
                if (note.HasNote) note.Release();
            _active.Clear();
        }

        private void OnDestroy() => _pool?.GetPool().Clear();

        private void FixedUpdate() => UpdateNotes();

        private void UpdateNotes()
        {
            if (!_playing) return;

            var songTime = AudioSettings.dspTime - _songStartDsp;
            if (songTime < 0d) return;

            // A trigger may release a note after FixedUpdate; drop stale references before pool reuse.
            for (var i = _active.Count - 1; i >= 0; i--)
                if (!_active[i].HasNote) _active.RemoveAt(i);

            // Take every note due to spawn, including several in the same tick.
            while (_timeline.TryTakeDue(songTime, out var noteEvent))
            {
                var note = _pool.GetPool().Get();
                note.SetNote(noteEvent, _settings.GetNoteSprite(noteEvent),
                    _settings.GetNoteRadius(noteEvent.VisualType));
                note.Place(songTime, _layout.Current, _timeline.TravelSeconds);
                _active.Add(note);
            }

            // Iterate backward so hit or missed notes can be removed in place.
            for (var i = _active.Count - 1; i >= 0; i--)
            {
                var note = _active[i];
                if (!note.HasNote)
                {
                    _active.RemoveAt(i);
                    continue;
                }

                note.Place(songTime, _layout.Current, _timeline.TravelSeconds);
                // Count a miss when the note passes arrival time plus the hit window.
                if (songTime <= note.Note.SpawnTime + _timeline.TravelSeconds + _settings.HitWindowSeconds)
                    continue;

                note.Release();
                _active.RemoveAt(i);
                NoteMissed?.Invoke();
                return;
            }

            // Finish only after the chart, active notes, and audio clip have all ended.
            if (_timeline.IsDone && _active.Count == 0 &&
                songTime >= _music.clip.length)
            {
                _playing = false;
                SongFinished?.Invoke();
            }
        }
    }
}
