using System;
using System.Collections.Generic;
using Control.Note;
using UnityEngine;

namespace Game
{
    public class NoteManager : MonoBehaviour
    {
        private readonly List<NoteBehaviour> _active = new();
        private NoteEvent[] _chart;
        private NoteTimeline _timeline;
        private NotePool _pool;
        private PlayableSettings _settings;
        private ResponsiveLayout _layout;
        private AudioSource _music;
        private double _songStartDsp;
        private bool _playing;

        public event Action NoteMissed;
        public event Action SongFinished;

        public void Initialize(NoteEvent[] chart, NoteBehaviour prefab,
            PlayableSettings settings, ResponsiveLayout layout, AudioSource music)
        {
            _chart = chart;
            _pool = new NotePool(prefab, transform);
            _settings = settings;
            _layout = layout;
            _music = music;
        }

        public bool StartSong()
        {
            if (_chart == null || _settings == null || _settings.Song == null || _music == null)
                return false;

            StopSong();
            _timeline = new NoteTimeline(_chart, _settings.TravelSeconds);
            _music.clip = _settings.Song;
            _music.playOnAwake = false;
            _music.loop = false;
            _music.spatialBlend = 0f;
            _songStartDsp = AudioSettings.dspTime + 0.1d;
            _music.PlayScheduled(_songStartDsp);
            _playing = true;
            return true;
        }

        public void StopSong()
        {
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

            // A trigger can release a note after FixedUpdate. Remove it before the pool reuses it.
            for (var i = _active.Count - 1; i >= 0; i--)
                if (!_active[i].HasNote) _active.RemoveAt(i);

            while (_timeline.TryTakeDue(songTime, out var noteEvent))
            {
                var note = _pool.GetPool().Get();
                note.SetNote(noteEvent, _settings.GetNoteSprite(noteEvent),
                    _settings.GetNoteRadius(noteEvent.VisualType));
                note.Place(songTime, _layout.Current, _timeline.TravelSeconds);
                _active.Add(note);
            }

            for (var i = _active.Count - 1; i >= 0; i--)
            {
                var note = _active[i];
                if (!note.HasNote)
                {
                    _active.RemoveAt(i);
                    continue;
                }

                note.Place(songTime, _layout.Current, _timeline.TravelSeconds);
                if (songTime <= note.Note.SpawnTime + _timeline.TravelSeconds + _settings.HitWindowSeconds)
                    continue;

                note.Release();
                _active.RemoveAt(i);
                NoteMissed?.Invoke();
                return;
            }

            if (_timeline.IsDone && _active.Count == 0 &&
                songTime >= _music.clip.length)
            {
                _playing = false;
                SongFinished?.Invoke();
            }
        }
    }
}
