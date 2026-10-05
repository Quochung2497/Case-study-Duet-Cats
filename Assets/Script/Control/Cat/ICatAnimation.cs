using System;
using Control.Note;

namespace Control.Cat
{
    public interface ICatAnimation
    {
        void PlayIdle(Action onComplete);
        void PlayPlaying();
        void PlayHit(NoteVisualType type, Action onComplete);
    }
}
