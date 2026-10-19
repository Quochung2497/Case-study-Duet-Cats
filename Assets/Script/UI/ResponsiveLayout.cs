using System;
using UnityEngine;
using Utility.DependencyInjection;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game
{
    [ExecuteAlways]
    public class ResponsiveLayout : MonoBehaviour, IResponsiveLayout
    {
        [SerializeField] private bool showGizmos = true;
        [Inject] private SceneVisualSettings _settings;
        [Inject] private LayoutSceneRefs _refs;
        [Inject] private IViewport _viewport;

        private LayoutData _data;
        private bool _hasData;
        private Rect _pixelRect;
#if UNITY_EDITOR
        private PlayableInstaller _editorInstaller;
#endif

        public event Action Changed;
        public LayoutData Current => _data;

        public bool IsPortrait => _viewport?.IsPortrait ?? false;

        private void OnEnable()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                _editorInstaller = FindFirstObjectByType<PlayableInstaller>();
                _viewport = _editorInstaller?.Viewport;
            }
#endif
            if (_viewport != null)
                _viewport.Changed += OnViewportChanged;
            RecalculateLayout();
        }

        private void OnDisable()
        {
            if (_viewport != null)
                _viewport.Changed -= OnViewportChanged;
        }

        private void OnValidate() => _hasData = false;
        private void OnViewportChanged(bool _) => RecalculateLayout();

        private SceneVisualSettings GetSettings()
        {
#if UNITY_EDITOR
            // Injection is unavailable in Edit mode, so use the installer for previews.
            if (!Application.isPlaying)
                return _editorInstaller != null ? _editorInstaller.SceneVisuals : null;
#endif
            return _settings;
        }

        private LayoutSceneRefs GetRefs()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return _editorInstaller != null ? _editorInstaller.SceneRefs : null;
#endif
            return _refs;
        }

        private bool TryCalculate(out LayoutData data, out LayoutSceneRefs refs, out Camera cam)
        {
            data = default;
            refs = GetRefs();
            cam = refs?.Camera;
            var settings = GetSettings();
            if (settings == null || cam == null || !cam.orthographic ||
                _viewport == null || !_viewport.HasViewport)
                return false;

            var profile = _viewport.IsPortrait ? settings.Portrait : settings.Landscape;
            data = LayoutMath.Calculate(cam.transform.position.x, cam.transform.position.y,
                cam.orthographicSize, cam.aspect, profile);
            return true;
        }

        public void RecalculateLayout()
        {
            if (!TryCalculate(out var next, out var refs, out var cam))
                return;

            // Reposition cats and notify listeners only when the layout changes.
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
            if (cat == null) return;
            var pos = cat.position;
            cat.position = new Vector3(x, y, pos.z);
        }

        private void OnDrawGizmos() => DrawLayoutGizmos();

        private void DrawLayoutGizmos()
        {
            if (!showGizmos || !TryCalculate(out var data, out _, out _))
                return;

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
}
