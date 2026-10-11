using System;
using Control.Note;
using UnityEngine;

namespace Game
{
    [Serializable]
    public sealed class HitVfxSettings
    {
        [Serializable]
        public struct HitVfxProfile
        {
            [Min(1)] public int count;
            [Min(0.01f)] public float size;
            [Min(0f)] public float speed;
            [Min(0.01f)] public float lifetime;
        }

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

        public float HitVfxTiltDegrees => hitVfxTiltDegrees;
        public float HitVfxSpreadDegrees => hitVfxSpreadDegrees;
        public float HitVfxAngleJitterDegrees => hitVfxAngleJitterDegrees;
        public float HitVfxSpeedVariance => hitVfxSpeedVariance;

        public HitVfxProfile GetHitVfx(NoteVisualType type)
        {
            // Select a particle profile per visual type, with Normal as the fallback.
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
            // Lolipop uses a gradient; other notes use the cat's primary color.
            if (type == NoteVisualType.LolipopLong && lolipopLongHitGradient != null)
                return lolipopLongHitGradient.Evaluate(t);

            return left ? leftHitColor : rightHitColor;
        }
    }
}
