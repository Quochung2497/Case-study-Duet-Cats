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
        private Sprite _defaultSprite;
        private bool _hasNote;

        public NoteEvent Note { get; private set; }
        public bool HasNote => _hasNote;

        private void Awake()
        {
            _sprite = gameObject.GetOrAdd<SpriteRenderer>();
            _defaultSprite = _sprite.sprite;
        }

        public void SetPool(IObjectPool<NoteBehaviour> pool) => _pool = pool;

        public void ResetForReuse()
        {
            _hasNote = false;
            Note = default;
            if (_sprite != null) _sprite.sprite = _defaultSprite;
        }

        public void SetNote(NoteEvent note, Sprite sprite)
        {
            Note = note;
            _hasNote = true;
            if (_sprite == null) _sprite = gameObject.GetOrAdd<SpriteRenderer>();
            if (sprite != null) _sprite.sprite = sprite;
        }

        public void Place(double songTime, ResponsiveLayout.LayoutData layout,
            float travelSeconds)
        {
            if (!_hasNote) return;

            var pos = transform.position;
            transform.position = new Vector3(
                layout.LaneX(Note.Lane),
                NoteTimeline.GetY(layout.SpawnY, layout.HitY, songTime,
                    Note.SpawnTime, travelSeconds),
                pos.z);
        }

        public void Release()
        {
            if (!_hasNote) return;
            _hasNote = false;
            _pool?.Release(this);
        }
    }
}
