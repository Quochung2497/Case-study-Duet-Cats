using System;
using Control.Note;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = "PlayableSettings", menuName = "Duet Cats/Playable Settings")]
    public class PlayableSettings : ScriptableObject
    {
        [Serializable]
        public struct LayoutProfile
        {
            [Tooltip("World-unit margin on each side of the camera.")]
            [Min(0f)] public float sideMargin;
            [Tooltip("Upper limit for the centered gameplay area's width in world units.")]
            [Min(0.1f)] public float maxPlayWidth;
            [Tooltip("Visible gap between the two cats at their closest positions.")]
            [Min(0f)] public float centerGap;
            [Tooltip("Approximate half-width of a cat, used to keep it within its half of the play area.")]
            [Min(0f)] public float catHalfWidth;
            [Tooltip("Default cat center as a fraction of playWidth from its outer edge.")]
            [Range(0f, 0.5f)] public float catX;
            [Tooltip("Outer note lane as a fraction of playWidth from its outer edge.")]
            [Range(0f, 0.5f)] public float outerLaneX;
            [Tooltip("Inner note lane as a fraction of playWidth from its outer edge.")]
            [Range(0f, 0.5f)] public float innerLaneX;
            [Tooltip("Cat origin height in the camera viewport: 0 = bottom, 1 = top.")]
            [Range(0f, 1f)] public float catY;
            [Tooltip("Note arrival height in the camera viewport.")]
            [Range(0f, 1f)] public float hitY;
            [Tooltip("Note spawn height in viewport units. Above 1 starts just off screen.")]
            public float spawnY;
        }

        [Header("Responsive layout")]
        [SerializeField] private LayoutProfile portrait;
        [SerializeField] private LayoutProfile landscape;

        [Header("Song and chart")]
        [SerializeField] private TextAsset chart;
        [SerializeField] private AudioClip song;
        [Min(0.01f)] [SerializeField] private float travelSeconds = 1.4f;
        [Min(0f)] [SerializeField] private float hitWindowSeconds = 0.2f;

        [Header("Background")]
        [SerializeField] private Sprite portraitBackground;
        [SerializeField] private Sprite landscapeBackground;

        [Header("Note sprites")]
        [SerializeField] private Sprite leftNormal;
        [SerializeField] private Sprite leftStrong;
        [SerializeField] private Sprite leftLong;
        [SerializeField] private Sprite rightNormal;
        [SerializeField] private Sprite rightStrong;
        [SerializeField] private Sprite rightLong;
        [SerializeField] private Sprite lolipopLong;

        public LayoutProfile Portrait => portrait;
        public LayoutProfile Landscape => landscape;
        public TextAsset Chart => chart;
        public AudioClip Song => song;
        public float TravelSeconds => travelSeconds;
        public float HitWindowSeconds => hitWindowSeconds;
        public Sprite PortraitBackground => portraitBackground;
        public Sprite LandscapeBackground => landscapeBackground;

        public Sprite GetNoteSprite(NoteEvent note)
        {
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
