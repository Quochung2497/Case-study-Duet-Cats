using UnityEngine;
using Utility;
using Utility.DependencyInjection;

namespace Game
{
    public class BackgroundBehaviour : MonoBehaviour
    {
        [Inject] private SceneVisualSettings _settings;
        [Inject] private LayoutSceneRefs _refs;
        [Inject] private IViewport _viewport;

        private SpriteRenderer _renderer;
        private Sprite _sprite;
        private float _aspect = -1f;
        private float _orthoSize = -1f;
        private Vector3 _cameraPos;

        private void Awake() => SetupRenderer();

        private void OnEnable()
        {
            if (_viewport == null) return;
            _viewport.Changed += ApplyBackground;
            if (_viewport.HasViewport) ApplyBackground(_viewport.IsPortrait);
        }

        private void OnDisable()
        {
            if (_viewport != null) _viewport.Changed -= ApplyBackground;
        }

        private void SetupRenderer()
        {
            _renderer = gameObject.GetOrAdd<SpriteRenderer>();
            _renderer.sortingOrder = -100;
        }

        private void ApplyBackground(bool isPortrait)
        {
            var cam = _refs?.Camera;
            if (_settings == null || cam == null || !cam.orthographic)
                return;

            // Choose the background sprite for the current screen orientation.
            var sprite = isPortrait
                ? _settings.PortraitBackground
                : _settings.LandscapeBackground;
            if (sprite == null)
                return;

            var camPos = cam.transform.position;
            // Recalculate scale only when the sprite or camera changes.
            if (sprite == _sprite && Mathf.Approximately(cam.aspect, _aspect) &&
                Mathf.Approximately(cam.orthographicSize, _orthoSize) &&
                camPos == _cameraPos)
                return;

            _renderer ??= gameObject.GetOrAdd<SpriteRenderer>();
            _renderer.sprite = sprite;

            var camHeight = cam.orthographicSize * 2f;
            var camWidth = camHeight * cam.aspect;
            var size = sprite.bounds.size;
            // Use the larger scale factor to cover the camera in both dimensions.
            var scale = Mathf.Max(camWidth / size.x, camHeight / size.y);

            var pos = transform.position;
            transform.position = new Vector3(camPos.x, camPos.y, pos.z);
            transform.localScale = new Vector3(scale, scale, 1f);

            _sprite = sprite;
            _aspect = cam.aspect;
            _orthoSize = cam.orthographicSize;
            _cameraPos = camPos;
        }
    }
}
