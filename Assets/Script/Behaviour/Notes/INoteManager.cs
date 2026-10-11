using System;
using Control.Note;

namespace Game
{
    public interface INoteManager
    {
        event Action NoteMissed;
        event Action SongFinished;

        void Initialize(NoteEvent[] chart, NoteBehaviour prefab,
            INoteSettings settings, ResponsiveLayout layout, IScoreManager score);
        bool StartSong();
        void StopSong();
    }
}
