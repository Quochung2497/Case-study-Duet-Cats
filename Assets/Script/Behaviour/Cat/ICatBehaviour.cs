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

        void ConfigureStateMachine(IStateMachine<CatState> fsm, CatHitState hitState,
            CatResultState resultState);
        void Initialize(ICatAction action, bool left, IInputReader input, ResponsiveLayout layout);
        void OnNoteHit(NoteVisualType type);
        void StartPlaying();
        void StopPlaying();
        void ShowResult(bool won);
        void SetVisible(bool visible);
    }
}
