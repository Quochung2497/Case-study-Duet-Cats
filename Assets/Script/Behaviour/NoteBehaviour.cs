using Control.Note;
using UnityEngine;
using UnityEngine.Pool;
using Utility;

namespace Game
{
    public class NoteBehaviour : MonoBehaviour, IPoolObject<NoteBehaviour>
    {
        private IObjectPool<NoteBehaviour> _pool;
        private SpriteRenderer _sprite;
        private Rigidbody2D _rb;
        private CircleCollider2D _collider;
        private Sprite _defaultSprite;
        private bool _hasNote;
        private bool _placed;

        public NoteEvent Note { get; private set; }
        public bool HasNote => _hasNote;

        private void Awake() => CacheComponents();

        private void CacheComponents()
        {
            _sprite = gameObject.GetOrAdd<SpriteRenderer>();
            _defaultSprite = _sprite.sprite;
            TryGetComponent(out _rb);
            TryGetComponent(out _collider);
        }

        public void SetPool(IObjectPool<NoteBehaviour> pool) => _pool = pool;

        public void ResetForReuse()
        {
            _hasNote = false;
            _placed = false;
            Note = default;
            if (_sprite != null) _sprite.sprite = _defaultSprite;
        }

        public void SetNote(NoteEvent note, Sprite sprite, float radius)
        {
            Note = note;
            _hasNote = true;
            _placed = false;
            if (_sprite == null) _sprite = gameObject.GetOrAdd<SpriteRenderer>();
            if (sprite != null) _sprite.sprite = sprite;
            if (_collider != null) _collider.radius = radius;
        }

        public void Place(double songTime, ResponsiveLayout.LayoutData layout,
            float travelSeconds)
        {
            if (!_hasNote || _rb == null) return;

            var pos = new Vector2(
                layout.LaneX(Note.Lane),
                NoteTimeline.GetY(layout.SpawnY, layout.HitY, songTime,
                    Note.SpawnTime, travelSeconds));

            if (!_placed)
            {
                _rb.position = pos;
                _placed = true;
            }
            else
            {
                _rb.MovePosition(pos);
            }
        }

        private void OnTriggerEnter2D(Collider2D other) => HandleTriggerEnter(other);

        private void HandleTriggerEnter(Collider2D other)
        {
            if (!_hasNote || !other.TryGetComponent<CatBehaviour>(out var cat) ||
                cat.IsLeft != (Note.Lane < 2)) return;

            Debug.Log($"Hit note {Note.Id} ({Note.VisualType}, lane {Note.Lane}) with {cat.name}", this);
            Release();
        }

        public void Release()
        {
            if (!_hasNote) return;
            _hasNote = false;
            _pool?.Release(this);
        }
    }
}
