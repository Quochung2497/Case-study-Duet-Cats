using System;
using Control.Note;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = "PlayableSettings", menuName = "Duet Cats/Playable Settings")]
    public class PlayableSettings : ScriptableObject
    {
        #region Configuration types

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

        [Serializable]
        public struct HitVfxProfile
        {
            [Min(1)] public int count;
            [Min(0.01f)] public float size;
            [Min(0f)] public float speed;
            [Min(0.01f)] public float lifetime;
        }

        #endregion

        #region Inspector fields

        [Header("Hit VFX")]
        [SerializeField] private Color leftHitColor = new Color(1f, 0.78f, 0.19f);
        [SerializeField] private Color rightHitColor = new Color(1f, 0.38f, 0.65f);
        [SerializeField] private Gradient lolipopLongHitGradient = new Gradient();
        [Tooltip("Angle toward the center from straight up. Mirrored for the right cat.")]
        [Range(0f, 90f)] [SerializeField] private float hitVfxTiltDegrees = 35f;
        [Tooltip("Total width of the upward particle fan in degrees.")]
        [Range(0f, 360f)] [SerializeField] private float hitVfxSpreadDegrees = 70f;
        [Tooltip("Random angle offset for each particle in degrees.")]
        [Range(0f, 45f)] [SerializeField] private float hitVfxAngleJitterDegrees = 7f;
        [Tooltip("Random speed variation around each note profile's Speed. 0.15 means plus or minus 15%.")]
        [Range(0f, 1f)] [SerializeField] private float hitVfxSpeedVariance = 0.15f;
        [SerializeField] private HitVfxProfile normalHitVfx;
        [SerializeField] private HitVfxProfile strongHitVfx;
        [SerializeField] private HitVfxProfile longHitVfx;
        [SerializeField] private HitVfxProfile lolipopLongHitVfx;

        [Header("Hit text")]
        [SerializeField] private TMPro.TMP_FontAsset hitTextFont;
        [SerializeField] private string[] hitWords = { "Tasty!", "Sweet!", "Yummy!" };
        [SerializeField] private Color hitTextColor = Color.white;
        [Min(0.01f)] [SerializeField] private float hitTextDuration = 0.7f;
        [Min(0f)] [SerializeField] private float hitTextOffsetY = 0.45f;
        [Min(0f)] [SerializeField] private float hitTextRise = 0.15f;
        [Min(0.1f)] [SerializeField] private float hitTextWorldWidth = 1.8f;
        [Min(1f)] [SerializeField] private float hitTextFontSize = 40f;

        [Header("Responsive layout")]
        [SerializeField] private LayoutProfile portrait;
        [SerializeField] private LayoutProfile landscape;

        [Header("Song and chart")]
        [SerializeField] private TextAsset chart;
        [SerializeField] private AudioClip song;
        [Min(0.01f)] [SerializeField] private float travelSeconds = 1.4f;
        [Min(0f)] [SerializeField] private float hitWindowSeconds = 0.2f;

        [Header("Game flow")]
        [Min(0f)] [SerializeField] private float resultSeconds = 3f;

        [Header("Background")]
        [SerializeField] private Sprite portraitBackground;
        [SerializeField] private Sprite landscapeBackground;

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

        #endregion

        #region Score values

        [Header("Score")]
        [Min(0)] [SerializeField] private int normalPoints = 2;
        [Min(0)] [SerializeField] private int strongPoints = 5;
        [Min(0)] [SerializeField] private int longPoints = 10;
        [Min(0)] [SerializeField] private int lolipopLongPoints = 10;

        #endregion

        #region Public settings

        public LayoutProfile Portrait => portrait;
        public LayoutProfile Landscape => landscape;
        public TextAsset Chart => chart;
        public AudioClip Song => song;
        public float TravelSeconds => travelSeconds;
        public float HitWindowSeconds => hitWindowSeconds;
        public float ResultSeconds => resultSeconds;
        public float HitVfxTiltDegrees => hitVfxTiltDegrees;
        public float HitVfxSpreadDegrees => hitVfxSpreadDegrees;
        public float HitVfxAngleJitterDegrees => hitVfxAngleJitterDegrees;
        public float HitVfxSpeedVariance => hitVfxSpeedVariance;
        public TMPro.TMP_FontAsset HitTextFont => hitTextFont;
        public string[] HitWords => hitWords;
        public Color HitTextColor => hitTextColor;
        public float HitTextDuration => hitTextDuration;
        public float HitTextOffsetY => hitTextOffsetY;
        public float HitTextRise => hitTextRise;
        public float HitTextWorldWidth => hitTextWorldWidth;
        public float HitTextFontSize => hitTextFontSize;
        public Sprite PortraitBackground => portraitBackground;
        public Sprite LandscapeBackground => landscapeBackground;

        #endregion

        #region Note presentation

        public HitVfxProfile GetHitVfx(NoteVisualType type)
        {
            switch (type)
            {
                case NoteVisualType.Strong: return strongHitVfx;
                case NoteVisualType.Long: return longHitVfx;
                case NoteVisualType.LolipopLong: return lolipopLongHitVfx;
                default: return normalHitVfx;
            }
        }

        public Color GetHitVfxColor(NoteVisualType type, bool left, float t)
        {
            if (type == NoteVisualType.LolipopLong && lolipopLongHitGradient != null)
                return lolipopLongHitGradient.Evaluate(t);

            return left ? leftHitColor : rightHitColor;
        }

        public float GetNoteRadius(NoteVisualType type)
        {
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

        #endregion

        #region Score

        public int GetPoints(NoteVisualType type)
        {
            switch (type)
            {
                case NoteVisualType.Strong: return strongPoints;
                case NoteVisualType.Long: return longPoints;
                case NoteVisualType.LolipopLong: return lolipopLongPoints;
                default: return normalPoints;
            }
        }

        #endregion
    }
}
