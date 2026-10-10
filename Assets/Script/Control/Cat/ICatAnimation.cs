using System;
using Control.Note;

namespace Control.Cat
{
    // States request animations; CatBehaviour chooses the actual Spine clip.
    public interface ICatAnimation
    {
        void PlayIdle(Action onComplete);
        void PlayPlaying();
        void PlayHit(NoteVisualType type, Action onComplete);
        void PlayResult(bool won);
    }
}
