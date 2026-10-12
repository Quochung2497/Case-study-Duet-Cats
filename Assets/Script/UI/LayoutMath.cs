using System;

namespace Game
{
    // Pure calculation: camera dimensions and a profile produce world-space layout data.
    internal static class LayoutMath
    {
        public static LayoutData Calculate(float camX, float camY,
            float orthoSize, float aspect, SceneVisualSettings.LayoutProfile p)
        {
            var height = Math.Max(0.1f, orthoSize * 2f);
            var cameraWidth = height * Math.Max(0.01f, aspect);
            var availableWidth = Math.Max(0.1f, cameraWidth - 2f * Math.Max(0f, p.sideMargin));
            var playWidth = Math.Min(availableWidth, Math.Max(0.1f, p.maxPlayWidth));
            var playLeft = camX - playWidth * 0.5f;
            var playRight = camX + playWidth * 0.5f;
            var gap = Math.Min(Math.Max(0f, p.centerGap), playWidth * 0.35f);
            var halfCat = Math.Min(Math.Max(0f, p.catHalfWidth), (playWidth - gap) * 0.25f);

            var leftMin = playLeft + halfCat;
            var leftMax = camX - gap * 0.5f - halfCat;
            var rightMin = camX + gap * 0.5f + halfCat;
            var rightMax = playRight - halfCat;
            var defaultX = Clamp(p.catX, 0f, 0.5f);
            var outer = Clamp(p.outerLaneX, 0f, 0.5f);
            var inner = Clamp(p.innerLaneX, 0f, 0.5f);
            // Preserve lane order even when the profile values are reversed.
            if (outer > inner)
            {
                var tmp = outer;
                outer = inner;
                inner = tmp;
            }

            var bottom = camY - height * 0.5f;
            return new LayoutData(
                playLeft, playRight, leftMin, leftMax, rightMin, rightMax,
                Clamp(playLeft + playWidth * defaultX, leftMin, leftMax),
                Clamp(playRight - playWidth * defaultX, rightMin, rightMax),
                bottom + height * Clamp(p.catY, 0f, 1f),
                bottom + height * Clamp(p.hitY, 0f, 1f),
                bottom + height * p.spawnY,
                playLeft + playWidth * outer,
                playLeft + playWidth * inner,
                playRight - playWidth * inner,
                playRight - playWidth * outer);
        }

        public static float Clamp(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }
    }
}
