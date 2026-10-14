using System;
using UnityEngine;

namespace Game
{
    // Pure calculations for gameplay geometry and responsive UI groups.
    public static class LayoutMath
    {
        public static LayoutData Calculate(float camX, float camY,
            float orthoSize, float aspect, SceneVisualSettings.LayoutProfile profile)
        {
            // Orthographic size is half the visible height; aspect converts height to width.
            // Remove both side margins, then cap the centered play area to the profile's width.
            var height = Math.Max(0.1f, orthoSize * 2f);
            var cameraWidth = height * Math.Max(0.01f, aspect);
            var availableWidth = Math.Max(0.1f, cameraWidth - 2f * Math.Max(0f, profile.sideMargin));
            var playWidth = Math.Min(availableWidth, Math.Max(0.1f, profile.maxPlayWidth));
            var playLeft = camX - playWidth * 0.5f;
            var playRight = camX + playWidth * 0.5f;

            // Reserve a gap between the cats. Each side must still fit a cat's full width.
            var gap = Math.Min(Math.Max(0f, profile.centerGap), playWidth * 0.35f);
            var halfCat = Math.Min(Math.Max(0f, profile.catHalfWidth), (playWidth - gap) * 0.25f);
            // These limits constrain cat centers, leaving half a cat's width at each edge.
            var leftMin = playLeft + halfCat;
            var leftMax = camX - gap * 0.5f - halfCat;
            var rightMin = camX + gap * 0.5f + halfCat;
            var rightMax = playRight - halfCat;
            // X values are fractions of the full play width; 0.5 is the center line.
            var defaultX = Clamp(profile.catX, 0f, 0.5f);
            var outer = Clamp(profile.outerLaneX, 0f, 0.5f);
            var inner = Clamp(profile.innerLaneX, 0f, 0.5f);
            // Preserve lane order even when the profile values are reversed.
            if (outer > inner)
            {
                var tmp = outer;
                outer = inner;
                inner = tmp;
            }

            // Convert normalized positions to world coordinates and mirror the right-side lanes.
            // Spawn Y may exceed 1 so notes can start above the visible camera area.
            var bottom = camY - height * 0.5f;
            return new LayoutData(
                playLeft, playRight, leftMin, leftMax, rightMin, rightMax,
                Clamp(playLeft + playWidth * defaultX, leftMin, leftMax),
                Clamp(playRight - playWidth * defaultX, rightMin, rightMax),
                bottom + height * Clamp(profile.catY, 0f, 1f),
                bottom + height * Clamp(profile.hitY, 0f, 1f),
                bottom + height * profile.spawnY,
                playLeft + playWidth * outer,
                playLeft + playWidth * inner,
                playRight - playWidth * inner,
                playRight - playWidth * outer);
        }

        public static UIRectLayout CalculateUI(UIRectLayout portrait, UIRectLayoutRules rules,
            Rect parentRect, Rect viewportRect, bool isPortrait)
        {
            // Both rectangles use the same parent-local UI units, rather than screen pixels.
            var parentRatio = new Vector2(parentRect.width / rules.referenceParentSize.x,
                parentRect.height / rules.referenceParentSize.y);
            var bounds = GetContentBounds(portrait, rules);
            var available = GetAvailableRect(parentRect, viewportRect, rules);
            var factor = GetScaleFactor(rules, parentRatio, bounds, available, isPortrait);
            var offset = GetPositionOffset(portrait, rules.positionMode, parentRatio, factor);
            if (!isPortrait && rules.clampLandscapePosition)
                offset = ClampOffset(portrait, parentRect, bounds, available, offset, factor);

            // Always derive from the saved portrait value, so repeated resizing cannot accumulate scale.
            // Only the group's XY position/scale change; child animation values remain local to it.
            var result = portrait;
            result.anchoredPosition = new Vector3(offset.x, offset.y, portrait.anchoredPosition.z);
            result.scale = new Vector3(portrait.scale.x * factor, portrait.scale.y * factor, portrait.scale.z);
            return result;
        }

        public static Rect GetRectInLocalSpace(Vector3[] worldCorners, Matrix4x4 worldToLocal)
        {
            // Convert the Canvas corners into the group's parent space before comparing rectangles.
            // The matrix includes the parent's position, rotation and scale, including its ancestors.
            var min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            var max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            foreach (var corner in worldCorners)
            {
                var point = (Vector2)worldToLocal.MultiplyPoint3x4(corner);
                min = Vector2.Min(min, point);
                max = Vector2.Max(max, point);
            }
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        private static Rect GetContentBounds(UIRectLayout portrait, UIRectLayoutRules rules)
        {
            // Authored bounds describe the whole group, including the largest animation frame.
            // This calculation does not inspect children or sample their animation clips.
            var sourceBounds = rules.contentBounds;
            if (sourceBounds.width <= 0f || sourceBounds.height <= 0f)
            {
                // For stretched anchors: rect size = parent size * anchor span + sizeDelta.
                // The pivot locates the local origin inside that rectangle.
                var size = Vector2.Scale(rules.referenceParentSize,
                    portrait.anchorMax - portrait.anchorMin) + portrait.sizeDelta;
                sourceBounds = new Rect(-Vector2.Scale(size, portrait.pivot), size);
            }
            return TransformBounds(sourceBounds, portrait.scale, portrait.rotation.z);
        }

        private static Rect GetAvailableRect(Rect parentRect, Rect viewportRect, UIRectLayoutRules rules)
        {
            // A parent panel can extend beyond the Canvas; only their overlapping area is visible.
            var visible = Rect.MinMaxRect(Mathf.Max(parentRect.xMin, viewportRect.xMin),
                Mathf.Max(parentRect.yMin, viewportRect.yMin),
                Mathf.Min(parentRect.xMax, viewportRect.xMax),
                Mathf.Min(parentRect.yMax, viewportRect.yMax));
            // Fractions reserve space equally at both edges: height 0.9 leaves 5% above and below.
            var availableSize = Vector2.Scale(visible.size,
                new Vector2(rules.availableWidth, rules.availableHeight));
            return new Rect(visible.center - availableSize * 0.5f, availableSize);
        }

        private static float GetScaleFactor(UIRectLayoutRules rules, Vector2 parentRatio,
            Rect bounds, Rect available, bool isPortrait)
        {
            var mode = isPortrait ? rules.portraitScaleMode : rules.landscapeScaleMode;
            // KeepSize starts at 1. The smaller width/height ratio fits both dimensions uniformly.
            var factor = 1f;
            if (mode == UIRectScaleMode.FitParent)
                factor = Mathf.Min(parentRatio.x, parentRatio.y);
            else if (mode == UIRectScaleMode.FitContent)
                factor = Mathf.Min(available.width / bounds.width, available.height / bounds.height);
            // Caps limit enlargement even when the screen has enough space for a larger group.
            factor = Mathf.Max(0f, Mathf.Min(factor, rules.maxScale));
            if (!isPortrait) factor = Mathf.Min(factor, rules.landscapeMaxScale);
            return factor;
        }

        private static Vector2 GetPositionOffset(UIRectLayout portrait, UIRectPositionMode mode,
            Vector2 parentRatio, float factor)
        {
            // anchoredPosition is an offset from the anchor, not an absolute position in the parent.
            var offset = new Vector2(portrait.anchoredPosition.x, portrait.anchoredPosition.y);
            if (mode == UIRectPositionMode.ScaleWithContent)
                offset *= factor;
            else if (mode == UIRectPositionMode.RelativeToParent)
                offset = Vector2.Scale(offset, parentRatio);
            return offset;
        }

        private static Vector2 ClampOffset(UIRectLayout portrait, Rect parentRect,
            Rect bounds, Rect available, Vector2 offset, float factor)
        {
            // Resolve the anchor to parent-local coordinates. For stretched anchors, Unity uses
            // the pivot to interpolate between anchorMin and anchorMax on each axis.
            var anchor = parentRect.min + Vector2.Scale(parentRect.size, new Vector2(
                Mathf.Lerp(portrait.anchorMin.x, portrait.anchorMax.x, portrait.pivot.x),
                Mathf.Lerp(portrait.anchorMin.y, portrait.anchorMax.y, portrait.pivot.y)));
            // Require pivot + scaled bounds to stay inside available on both sides.
            // Example: pivot + bounds.xMin * factor >= available.xMin gives the lower pivot limit.
            var position = anchor + offset;
            position.x = ClampPosition(position.x, available.xMin - bounds.xMin * factor,
                available.xMax - bounds.xMax * factor);
            position.y = ClampPosition(position.y, available.yMin - bounds.yMin * factor,
                available.yMax - bounds.yMax * factor);
            return position - anchor;
        }

        private static float Clamp(float value, float min, float max) =>
            Math.Max(min, Math.Min(max, value));

        // An oversized group has no valid interval; center it instead of clamping to reversed limits.
        private static float ClampPosition(float position, float min, float max) =>
            min <= max ? Mathf.Clamp(position, min, max) : (min + max) * 0.5f;

        private static Rect TransformBounds(Rect source, Vector3 scale, float degrees)
        {
            // Apply the group's baseline scale and Z rotation to all four corners, then enclose them.
            // A rotated rectangle needs a wider/taller envelope; this is separate from child rotation.
            var radians = degrees * Mathf.Deg2Rad;
            var cos = Mathf.Cos(radians);
            var sin = Mathf.Sin(radians);
            var min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            var max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            for (var i = 0; i < 4; i++)
            {
                var x = (i % 2 == 0 ? source.xMin : source.xMax) * scale.x;
                var y = (i < 2 ? source.yMin : source.yMax) * scale.y;
                // Standard 2D rotation: x' = x*cos - y*sin; y' = x*sin + y*cos.
                var point = new Vector2(x * cos - y * sin, x * sin + y * cos);
                min = Vector2.Min(min, point);
                max = Vector2.Max(max, point);
            }
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }
    }
}
