using System;
using UnityEngine;

namespace Game
{
    public enum UIRectScaleMode { KeepSize, FitParent, FitContent }
    public enum UIRectPositionMode { KeepAnchorOffset, ScaleWithContent, RelativeToParent }

    [Serializable]
    public class UIRectLayoutRules
    {
        [Tooltip("Parent size in UI units when the portrait baseline was authored.")]
        public Vector2 referenceParentSize = new(1080f, 2280f);
        public UIRectScaleMode portraitScaleMode = UIRectScaleMode.FitParent;
        public UIRectScaleMode landscapeScaleMode = UIRectScaleMode.FitContent;
        public UIRectPositionMode positionMode = UIRectPositionMode.RelativeToParent;

        [Range(0.01f, 1f)] public float availableWidth = 1f;
        [Range(0.01f, 1f)] public float availableHeight = 1f;
        [Min(0.01f)] public float maxScale = 1f;
        [Min(0.01f)] public float landscapeMaxScale = 1f;
        public bool clampLandscapePosition = true;

        [Tooltip("Local bounds at the portrait baseline, including animation peaks. Zero size uses the group's own rect.")]
        public Rect contentBounds;
    }
}
