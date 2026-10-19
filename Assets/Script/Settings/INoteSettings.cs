using Control.Note;
using UnityEngine;

namespace Game
{
    // Song selection is separate from these note timing and presentation rules.
    public interface INoteSettings
    {
        float TravelSeconds { get; }
        float HitWindowSeconds { get; }
        string CatLayerName { get; }
        float GetNoteRadius(NoteVisualType type);
        Sprite GetNoteSprite(NoteEvent note);
    }
}
