using System;
using TMPro;
using UnityEngine;

namespace Game
{
    [Serializable]
    public sealed class HitTextSettings
    {
        [SerializeField] private TMP_FontAsset hitTextFont;
        [SerializeField] private string[] hitWords = { "Tasty!", "Sweet!", "Yummy!" };
        [SerializeField] private Color hitTextColor = Color.white;
        [Min(0.01f)] [SerializeField] private float hitTextDuration = 0.7f;
        [Min(0f)] [SerializeField] private float hitTextOffsetY = 0.45f;
        [Min(0f)] [SerializeField] private float hitTextRise = 0.15f;
        [Min(0.1f)] [SerializeField] private float hitTextWorldWidth = 1.8f;
        [Min(1f)] [SerializeField] private float hitTextFontSize = 40f;

        public TMP_FontAsset HitTextFont => hitTextFont;
        public string[] HitWords => hitWords;
        public Color HitTextColor => hitTextColor;
        public float HitTextDuration => hitTextDuration;
        public float HitTextOffsetY => hitTextOffsetY;
        public float HitTextRise => hitTextRise;
        public float HitTextWorldWidth => hitTextWorldWidth;
        public float HitTextFontSize => hitTextFontSize;
    }
}
