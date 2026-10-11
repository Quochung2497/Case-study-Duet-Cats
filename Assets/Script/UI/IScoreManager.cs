using System;
using Control.Note;

namespace Game
{
    public interface IScoreManager
    {
        int Score { get; }
        event Action ScoreChanged;

        void AddPoints(NoteVisualType type);
        void ResetScore();
    }
}
