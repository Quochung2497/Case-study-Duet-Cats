using System;
using Control.Note;
using UnityEngine;
using Utility;
using Utility.DependencyInjection;

namespace Game
{
    public class ScoreManager : Singleton<ScoreManager>
    {
        // Score has one owner; UI listens for changes and reads the Score property.
        [Inject] private PlayableSettings _settings;

        public int Score { get; private set; }
        public event Action ScoreChanged;

        public void AddPoints(NoteVisualType type)
        {
            if (_settings == null)
            {
                Debug.LogError("ScoreManager needs PlayableSettings.", this);
                return;
            }

            // PlayableSettings defines the point value for each note type.
            Score += _settings.GetPoints(type);
            ScoreChanged?.Invoke();
        }

        public void ResetScore()
        {
            // BeginPlaying resets the score; the event refreshes the UI to zero.
            Score = 0;
            ScoreChanged?.Invoke();
        }
    }
}
