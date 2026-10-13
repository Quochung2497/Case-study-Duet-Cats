using Control.Note;
using UnityEngine;
using UnityEngine.Pool;
using Utility;

namespace Game
{
    public class NoteBehaviour : MonoBehaviour, INoteBehaviour, IPoolObject<NoteBehaviour>
    {
        private int _catLayer;
        private IScoreManager _score;

        private IObjectPool<NoteBehaviour> _pool;
        private SpriteRenderer _sprite;
        private Rigidbody2D _rb;
        private CircleCollider2D _collider;
        private Sprite _defaultSprite;
        private bool _hasNote;
        private bool _placed;

        public NoteEvent Note { get; private set; }
        public bool HasNote => _hasNote;

        private void Awake()
        {
            CacheComponents();
        }

        private void CacheComponents()
        {
            _sprite = gameObject.GetOrAdd<SpriteRenderer>();
            _defaultSprite = _sprite.sprite;
            TryGetComponent(out _rb);
            TryGetComponent(out _collider);
        }

        public void SetPool(IObjectPool<NoteBehaviour> pool) => _pool = pool;

        public void Initialize(IObjectPool<NoteBehaviour> pool, IScoreManager score, string catLayerName)
        {
            SetPool(pool);
            _score = score;
            _catLayer = LayerMask.NameToLayer(catLayerName);
        }

        public void ResetForReuse()
        {
            // Clear data from the previous use before this note leaves the pool.
            _hasNote = false;
            _placed = false;
            Note = default;
            if (_sprite != null) _sprite.sprite = _defaultSprite;
        }

        public void SetNote(NoteEvent note, Sprite sprite, float radius)
        {
            // Reuse one prefab for all note types by changing its sprite and collider radius.
            Note = note;
            _hasNote = true;
            _placed = false;
            if (_sprite == null) _sprite = gameObject.GetOrAdd<SpriteRenderer>();
            if (sprite != null) _sprite.sprite = sprite;
            if (_collider != null) _collider.radius = radius;
        }

        public void Place(double songTime, LayoutData layout, INoteTimeline timeline)
        {
            if (!_hasNote || _rb == null) return;

            var pos = new Vector2(
                layout.LaneX(Note.Lane),
                timeline.GetY(layout.SpawnY, layout.HitY, songTime, Note.SpawnTime));

            // Set the initial position directly; use Rigidbody2D.MovePosition on later ticks.
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
            // Only the cat on the note's side may hit it.
            if (!_hasNote || other.gameObject.layer != _catLayer ||
                !other.TryGetComponent<ICatBehaviour>(out var cat) ||
                cat.IsLeft != (Note.Lane < 2)) return;

            var type = Note.VisualType;
            // Let the cat handle feedback and ScoreManager award points for the visual type.
            cat.OnNoteHit(type);
            _score.AddPoints(type);
            Release();
        }

        public void Release()
        {
            // _hasNote prevents returning the same instance to the pool twice.
            if (!_hasNote) return;
            _hasNote = false;
            _pool?.Release(this);
        }
    }
}
