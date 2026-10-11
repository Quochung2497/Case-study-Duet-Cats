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
        [Inject] private IScoreRules _rules;

        public int Score { get; private set; }
        public event Action ScoreChanged;

        public void AddPoints(NoteVisualType type)
        {
            if (_rules == null)
            {
                Debug.LogError("ScoreManager needs score rules.", this);
                return;
            }

            // Use the injected rule; tests can supply a fake through their injector.
            Score += _rules.GetPoints(type);
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
