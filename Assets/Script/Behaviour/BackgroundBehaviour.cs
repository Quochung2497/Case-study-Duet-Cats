using UnityEngine;
using Utility;
using Utility.DependencyInjection;

namespace Game
{
    public class BackgroundBehaviour : MonoBehaviour
    {
        [Inject] private PlayableSettings _settings;
        [Inject] private LayoutSceneRefs _refs;

        private SpriteRenderer _renderer;
        private Sprite _sprite;
        private float _aspect = -1f;
        private float _orthoSize = -1f;
        private Vector3 _cameraPos;

        private void Awake()
        {
            _renderer = gameObject.GetOrAdd<SpriteRenderer>();
            _renderer.sortingOrder = -100;
        }

        private void LateUpdate()
        {
            var cam = _refs?.Camera;
            if (_settings == null || cam == null || !cam.orthographic)
                return;

            var sprite = cam.aspect < 1f
                ? _settings.PortraitBackground
                : _settings.LandscapeBackground;
            if (sprite == null)
                return;

            var camPos = cam.transform.position;
            if (sprite == _sprite && Mathf.Approximately(cam.aspect, _aspect) &&
                Mathf.Approximately(cam.orthographicSize, _orthoSize) &&
                camPos == _cameraPos)
                return;

            _renderer ??= gameObject.GetOrAdd<SpriteRenderer>();
            _renderer.sprite = sprite;

            var camHeight = cam.orthographicSize * 2f;
            var camWidth = camHeight * cam.aspect;
            var size = sprite.bounds.size;
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
