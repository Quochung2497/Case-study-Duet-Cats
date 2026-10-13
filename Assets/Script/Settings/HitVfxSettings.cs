using System;
using Control.Note;
using UnityEngine;

namespace Game
{
    [Serializable]
    public class HitVfxSettings
    {
        [Serializable]
        public struct HitVfxProfile
        {
            [Min(1)] public int count;
            [Min(0.01f)] public float size;
            public ParticleSystem.MinMaxCurve speed;
            [Min(0.01f)] public float lifetime;
        }

        [SerializeField] private Color leftHitColor = new Color(1f, 0.78f, 0.19f);
        [SerializeField] private Color rightHitColor = new Color(1f, 0.38f, 0.65f);
        [SerializeField] private Gradient lolipopLongHitGradient = new Gradient();
        [SerializeField] private HitVfxProfile normalHitVfx;
        [SerializeField] private HitVfxProfile strongHitVfx;
        [SerializeField] private HitVfxProfile longHitVfx;
        [SerializeField] private HitVfxProfile lolipopLongHitVfx;

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

        public ParticleSystem.MinMaxGradient GetHitVfxStartColor(NoteVisualType type, bool left)
        {
            // Unity picks a gradient color per Lolipop particle; other notes use the cat's color.
            if (type == NoteVisualType.LolipopLong && lolipopLongHitGradient != null)
                return new ParticleSystem.MinMaxGradient(lolipopLongHitGradient)
                {
                    mode = ParticleSystemGradientMode.RandomColor
                };

            return new ParticleSystem.MinMaxGradient(left ? leftHitColor : rightHitColor);
        }
    }
}
