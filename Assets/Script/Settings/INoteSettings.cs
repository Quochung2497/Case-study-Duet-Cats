using Control.Note;
using UnityEngine;

namespace Game
{
    // Note setup and playback depend only on these chart, timing, and presentation rules.
    public interface INoteSettings
    {
        TextAsset Chart { get; }
        AudioClip Song { get; }
        float TravelSeconds { get; }
        float HitWindowSeconds { get; }
        string CatLayerName { get; }
        float GetNoteRadius(NoteVisualType type);
        Sprite GetNoteSprite(NoteEvent note);
    }
}
