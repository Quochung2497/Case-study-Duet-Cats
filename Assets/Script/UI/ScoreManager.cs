using System;
using Control.Note;
using UnityEngine;
using Utility;
using Utility.DependencyInjection;

namespace Game
{
    public class ScoreManager : Singleton<ScoreManager>
    {
        [Inject] private PlayableSettings _settings;

        public int Score { get; private set; }
        public event Action ScoreChanged;

        private void OnEnable()
        {
            // TODO: GameManager will reset the score when a new run begins.
            ResetScore();
        }

        public void AddPoints(NoteVisualType type)
        {
            if (_settings == null)
            {
                Debug.LogError("ScoreManager needs PlayableSettings.", this);
                return;
            }

            Score += _settings.GetPoints(type);
            ScoreChanged?.Invoke();
        }

        public void ResetScore()
        {
            Score = 0;
            ScoreChanged?.Invoke();
        }
    }
}
