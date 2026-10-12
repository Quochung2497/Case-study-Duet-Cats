using System;
using UnityEngine;

namespace Game
{
    [Serializable]
    public struct UIRectLayout
    {
        public Vector2 anchorMin;
        public Vector2 anchorMax;
        public Vector2 pivot;
        public Vector3 anchoredPosition;
        public Vector2 sizeDelta;
        public Vector3 rotation;
        public Vector3 scale;
    }
}
