using System;
using Control.Note;

namespace Game
{
    public interface ICatBehaviour
    {
        bool IsLeft { get; }
        event Action<NoteVisualType> NoteHit;

        void OnNoteHit(NoteVisualType type);
        void ResetForRun();
        void StartPlaying();
        void StopPlaying();
        void ShowResult(bool won);
        void SetVisible(bool visible);
    }
}
