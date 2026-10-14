using System;
using UnityEngine;

namespace Game
{
    [ExecuteAlways]
    [DefaultExecutionOrder(-900)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public class ViewportController : MonoBehaviour, IViewport
    {
        private RectTransform _rootRect;
        private Camera _camera;
        private Vector2 _canvasSize;
        private Rect _cameraPixelRect;
        private float _cameraAspect;
        private float _cameraOrthoSize;
        private Vector3 _cameraPosition;

        public event Action<bool> Changed;
        public bool HasViewport { get; private set; }
        public bool IsPortrait { get; private set; }

        private void OnEnable()
        {
            Initialize();
            HasViewport = false;
            CheckViewport();
        }

        private void Update() => CheckViewport();

        private void Initialize()
        {
            var root = GetComponent<Canvas>().rootCanvas;
            _rootRect = root != null ? root.transform as RectTransform : null;
            _camera = root != null ? root.worldCamera : null;
        }

        private void CheckViewport()
        {
            if (_rootRect == null || _camera == null) return;

            var size = _rootRect.rect.size;
            var scale = _rootRect.lossyScale;
            // CanvasScaler can map many screen pixels to one local UI unit, so pixelRect is not
            // a substitute for an uninitialized Canvas rect. Zero world scale also collapses its corners.
            if (size.x <= 0f || size.y <= 0f || scale.x == 0f || scale.y == 0f)
            {
                // Retry on the next check; becoming valid again must publish a fresh event.
                HasViewport = false;
                return;
            }
            var pixelRect = _camera.pixelRect;
            var aspect = _camera.aspect;
            var orthoSize = _camera.orthographicSize;
            var position = _camera.transform.position;
            if (HasViewport && _canvasSize == size && _cameraPixelRect == pixelRect &&
                Mathf.Approximately(_cameraAspect, aspect) &&
                Mathf.Approximately(_cameraOrthoSize, orthoSize) && _cameraPosition == position)
                return;

            _canvasSize = size;
            _cameraPixelRect = pixelRect;
            _cameraAspect = aspect;
            _cameraOrthoSize = orthoSize;
            _cameraPosition = position;
            IsPortrait = size.y > size.x;
            HasViewport = true;
            Changed?.Invoke(IsPortrait);
        }
    }
}
