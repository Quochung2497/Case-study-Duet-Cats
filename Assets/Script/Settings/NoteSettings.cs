using System;
using Control.Note;
using UnityEngine;

namespace Game
{
    [Serializable]
    public class NoteSettings : INoteSettings
    {
        [Header("Song and chart")]
        [SerializeField] private TextAsset chart;
        [SerializeField] private AudioClip song;
        [Min(0.01f)] [SerializeField] private float travelSeconds = 1.4f;
        [Min(0f)] [SerializeField] private float hitWindowSeconds = 0.2f;

        [Header("Note collider radii")]
        [Range(0.34f, 0.49f)] [SerializeField] private float normalNoteRadius = 0.34f;
        [Range(0.34f, 0.49f)] [SerializeField] private float strongNoteRadius = 0.49f;
        [Range(0.34f, 0.49f)] [SerializeField] private float longNoteRadius = 0.42f;
        [Range(0.34f, 0.49f)] [SerializeField] private float lolipopLongNoteRadius = 0.49f;

        [Header("Note sprites")]
        [SerializeField] private Sprite leftNormal;
        [SerializeField] private Sprite leftStrong;
        [SerializeField] private Sprite leftLong;
        [SerializeField] private Sprite rightNormal;
        [SerializeField] private Sprite rightStrong;
        [SerializeField] private Sprite rightLong;
        [SerializeField] private Sprite lolipopLong;

        public TextAsset Chart => chart;
        public AudioClip Song => song;
        public float TravelSeconds => travelSeconds;
        public float HitWindowSeconds => hitWindowSeconds;

        public float GetNoteRadius(NoteVisualType type)
        {
            // Match the collider radius to the selected visual type.
            switch (type)
            {
                case NoteVisualType.Strong: return strongNoteRadius;
                case NoteVisualType.Long: return longNoteRadius;
                case NoteVisualType.LolipopLong: return lolipopLongNoteRadius;
                default: return normalNoteRadius;
            }
        }

        public Sprite GetNoteSprite(NoteEvent note)
        {
            // Lolipop shares one sprite; other notes choose a side from the lane.
            if (note.VisualType == NoteVisualType.LolipopLong)
                return lolipopLong;

            var left = note.Lane < 2;
            switch (note.VisualType)
            {
                case NoteVisualType.Strong: return left ? leftStrong : rightStrong;
                case NoteVisualType.Long: return left ? leftLong : rightLong;
                default: return left ? leftNormal : rightNormal;
            }
        }
    }
}
