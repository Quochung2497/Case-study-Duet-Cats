using System;
using Control.Note;

namespace Game
{
    public interface INoteManager
    {
        event Action NoteMissed;
        event Action SongFinished;

        void Initialize(INoteTimeline timeline, INotePool pool,
            ISongPlaybackSettings song, INoteSettings settings, IResponsiveLayout layout);
        bool StartSong();
        void StopSong();
    }
}
