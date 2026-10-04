using System;
using UnityEngine;
using Utility.DependencyInjection;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game
{
    [ExecuteAlways]
    public class ResponsiveLayout : MonoBehaviour
    {
        // World-space values that note spawning and cat movement can read later.
        public readonly struct LayoutData
        {
            public readonly float PlayLeft, PlayRight, LeftMinX, LeftMaxX, RightMinX, RightMaxX;
            public readonly float LeftCatX, RightCatX, CatY, HitY, SpawnY;
            public readonly float Lane0X, Lane1X, Lane2X, Lane3X;

            public float PlayWidth => PlayRight - PlayLeft;

            public LayoutData(float playLeft, float playRight,
                float leftMinX, float leftMaxX, float rightMinX, float rightMaxX,
                float leftCatX, float rightCatX, float catY, float hitY, float spawnY,
                float lane0X, float lane1X, float lane2X, float lane3X)
            {
                PlayLeft = playLeft;
                PlayRight = playRight;
                LeftMinX = leftMinX;
                LeftMaxX = leftMaxX;
                RightMinX = rightMinX;
                RightMaxX = rightMaxX;
                LeftCatX = leftCatX;
                RightCatX = rightCatX;
                CatY = catY;
                HitY = hitY;
                SpawnY = spawnY;
                Lane0X = lane0X;
                Lane1X = lane1X;
                Lane2X = lane2X;
                Lane3X = lane3X;
            }

            public float LaneX(int lane)
            {
                switch (lane)
                {
                    case 0: return Lane0X; // left outer
                    case 1: return Lane1X; // left inner
                    case 2: return Lane2X; // right inner
                    case 3: return Lane3X; // right outer
                    default: throw new ArgumentOutOfRangeException(nameof(lane));
                }
            }

            public float ClampCatX(bool left, float x)
            {
                return left
                    ? LayoutMath.Clamp(x, LeftMinX, LeftMaxX)
                    : LayoutMath.Clamp(x, RightMinX, RightMaxX);
            }

            public bool SameAs(LayoutData other)
            {
                return PlayLeft == other.PlayLeft && PlayRight == other.PlayRight &&
                       LeftMinX == other.LeftMinX && LeftMaxX == other.LeftMaxX &&
                       RightMinX == other.RightMinX && RightMaxX == other.RightMaxX &&
                       LeftCatX == other.LeftCatX && RightCatX == other.RightCatX &&
                       CatY == other.CatY && HitY == other.HitY && SpawnY == other.SpawnY &&
                       Lane0X == other.Lane0X && Lane1X == other.Lane1X &&
                       Lane2X == other.Lane2X && Lane3X == other.Lane3X;
            }
        }

        [SerializeField] private bool showGizmos = true;

        [Inject] private PlayableSettings _settings;
        [Inject] private LayoutSceneRefs _refs;
        private LayoutData _data;
        private bool _hasData;
        private Rect _pixelRect;
        public event Action Changed;

        public LayoutData Current => _data;
        public bool IsPortrait
        {
            get
            {
                var cam = GetRefs()?.Camera;
                return cam != null && cam.aspect < 1f;
            }
        }

        private void OnEnable() => Refresh();
        private void Update() => Refresh();
        private void OnValidate() => _hasData = false;

        private PlayableSettings GetSettings()
        {
#if UNITY_EDITOR
            // Injector runs in Play mode. Scene Gizmos still need the selected asset in Edit mode.
            if (!Application.isPlaying)
            {
                var installer = FindFirstObjectByType<PlayableInstaller>();
                return installer != null ? installer.Settings : null;
            }
#endif
            return _settings;
        }

        private LayoutSceneRefs GetRefs()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                var installer = FindFirstObjectByType<PlayableInstaller>();
                return installer != null ? installer.SceneRefs : null;
            }
#endif
            return _refs;
        }

        public void Refresh()
        {
            var settings = GetSettings();
            var refs = GetRefs();
            var gameplayCamera = refs?.Camera;
            if (settings == null || gameplayCamera == null || !gameplayCamera.orthographic)
                return;

            var cam = gameplayCamera;
            var p = cam.aspect < 1f ? settings.Portrait : settings.Landscape;
            var next = LayoutMath.Calculate(cam.transform.position.x, cam.transform.position.y,
                cam.orthographicSize, cam.aspect, p);

            if (_hasData && _data.SameAs(next) && _pixelRect == cam.pixelRect)
                return;

            _data = next;
            _hasData = true;
            _pixelRect = cam.pixelRect;

            Place(refs.LeftCat, next.LeftCatX, next.CatY);
            Place(refs.RightCat, next.RightCatX, next.CatY);
            Changed?.Invoke();
        }

        public float ScreenToWorldX(float screenX, float screenY)
        {
            var cam = GetRefs()?.Camera;
            if (cam == null) return 0f;
            return cam.ScreenToWorldPoint(new Vector3(screenX, screenY, cam.nearClipPlane)).x;
        }

        private static void Place(Transform cat, float x, float y)
        {
            if (cat == null)
                return;

            var pos = cat.position;
            cat.position = new Vector3(x, y, pos.z);
        }

        private void OnDrawGizmos()
        {
            var settings = GetSettings();
            var refs = GetRefs();
            var gameplayCamera = refs?.Camera;
            if (!showGizmos || settings == null || gameplayCamera == null || !gameplayCamera.orthographic)
                return;

            // Draw from the live camera so a Simulator rotation previews correctly in Edit mode.
            var cam = gameplayCamera;
            var p = cam.aspect < 1f ? settings.Portrait : settings.Landscape;
            var data = LayoutMath.Calculate(cam.transform.position.x, cam.transform.position.y,
                cam.orthographicSize, cam.aspect, p);

            Gizmos.color = new Color(0.2f, 0.9f, 1f, 0.8f);
            var midY = (data.SpawnY + data.HitY) * 0.5f;
            Gizmos.DrawWireCube(new Vector3((data.PlayLeft + data.PlayRight) * 0.5f, midY),
                new Vector3(data.PlayWidth, Math.Abs(data.SpawnY - data.HitY), 0f));

            for (var lane = 0; lane < 4; lane++)
            {
                var x = data.LaneX(lane);
                Gizmos.DrawLine(new Vector3(x, data.SpawnY), new Vector3(x, data.HitY));
                Gizmos.DrawWireSphere(new Vector3(x, data.SpawnY), 0.12f);
#if UNITY_EDITOR
                Handles.Label(new Vector3(x, data.SpawnY + 0.14f), lane.ToString());
#endif
            }

            Gizmos.color = Color.red;
            Gizmos.DrawLine(new Vector3(data.PlayLeft, data.HitY),
                new Vector3(data.PlayRight, data.HitY));

            Gizmos.color = Color.green;
            Gizmos.DrawLine(new Vector3(data.LeftMinX, data.CatY),
                new Vector3(data.LeftMaxX, data.CatY));
            Gizmos.DrawLine(new Vector3(data.RightMinX, data.CatY),
                new Vector3(data.RightMaxX, data.CatY));
            Gizmos.DrawWireSphere(new Vector3(data.LeftCatX, data.CatY), 0.12f);
            Gizmos.DrawWireSphere(new Vector3(data.RightCatX, data.CatY), 0.12f);
        }
    }

    // Only numbers go in and out. This can later be moved to a pure C# file without changing the scene API.
    internal static class LayoutMath
    {
        public static ResponsiveLayout.LayoutData Calculate(float camX, float camY,
            float orthoSize, float aspect, PlayableSettings.LayoutProfile p)
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
            if (outer > inner)
            {
                var tmp = outer;
                outer = inner;
                inner = tmp;
            }

            var bottom = camY - height * 0.5f;
            return new ResponsiveLayout.LayoutData(
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
