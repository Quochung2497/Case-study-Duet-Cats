using Control.Note;
using UnityEngine;

namespace Game
{
    public interface INoteBehaviour
    {
        NoteEvent Note { get; }
        bool HasNote { get; }
        void SetNote(NoteEvent note, Sprite sprite, float radius);
        void Place(double songTime, LayoutData layout, INoteTimeline timeline);
    }
}
