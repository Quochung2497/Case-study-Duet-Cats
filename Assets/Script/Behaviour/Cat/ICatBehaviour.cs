using System;
using Control;
using Control.Cat;
using Control.Note;
using Input;

namespace Game
{
    public interface ICatBehaviour
    {
        bool IsLeft { get; }
        event Action<NoteVisualType> NoteHit;

        void Initialize(ICatAction action, IStateMachine<CatState> fsm, CatHitState hitState,
            CatResultState resultState, bool left, IInputReader input, ResponsiveLayout layout);
        void OnNoteHit(NoteVisualType type);
        void StartPlaying();
        void StopPlaying();
        void ShowResult(bool won);
        void SetVisible(bool visible);
    }
}
