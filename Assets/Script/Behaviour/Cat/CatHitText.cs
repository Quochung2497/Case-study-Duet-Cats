using Control.Note;
using TMPro;
using UnityEngine;
using Utility.DependencyInjection;

namespace Game
{
    public class CatHitText : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private TextMeshProUGUI label;

        [Inject] private PlayableSettings _settings;
        [Inject] private LayoutSceneRefs _scene;

        private CatBehaviour _cat;
        private RectTransform _rect;
        private int _lastWord = -1;
        private float _startedAt;
        private bool _showing;

        private void Awake()
        {
            _cat = GetComponentInParent<CatBehaviour>();
            if (_cat == null || _settings == null || _scene?.Camera == null ||
                _settings.HitTextFont == null || canvas == null || label == null)
            {
                Debug.LogError("CatHitText needs its cat, camera, font, canvas and label.", this);
                return;
            }

            _rect = canvas.GetComponent<RectTransform>();
            label.font = _settings.HitTextFont;
            label.fontSize = _settings.HitTextFontSize;
            label.raycastTarget = false;
            label.text = string.Empty;
            canvas.worldCamera = _scene.Camera;
        }

        private void OnEnable()
        {
            if (_cat != null) _cat.NoteHit += Show;
        }

        private void OnDisable()
        {
            if (_cat != null) _cat.NoteHit -= Show;
            _showing = false;
            if (label != null) label.text = string.Empty;
        }

        private void Show(NoteVisualType type)
        {
            var words = _settings?.HitWords;
            if (_rect == null || words == null || words.Length == 0) return;

            var index = Random.Range(0, words.Length);
            if (words.Length > 1 && index == _lastWord)
                index = (index + 1) % words.Length;
            _lastWord = index;

            label.text = words[index];
            _startedAt = Time.unscaledTime;
            _showing = true;
            UpdateText();
        }

        private void LateUpdate()
        {
            if (_showing) UpdateText();
        }

        private void UpdateText()
        {
            var t = (Time.unscaledTime - _startedAt) / _settings.HitTextDuration;
            if (t >= 1f)
            {
                label.text = string.Empty;
                _showing = false;
                return;
            }

            var scale = _settings.HitTextWorldWidth / _rect.rect.width;
            var parentScale = transform.lossyScale;
            _rect.localScale = new Vector3(
                scale / Mathf.Abs(parentScale.x),
                scale / Mathf.Abs(parentScale.y),
                scale / Mathf.Abs(parentScale.z));
            _rect.rotation = _scene.Camera.transform.rotation;
            _rect.position = transform.position +
                Vector3.up * (_settings.HitTextOffsetY + _settings.HitTextRise * t);

            var color = _settings.HitTextColor;
            color.a *= 1f - t;
            label.color = color;
        }
    }
}