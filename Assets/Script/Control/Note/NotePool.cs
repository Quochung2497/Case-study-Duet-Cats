using UnityEngine;
using Utility;

namespace Game
{
    public class NotePool : Pool<NoteBehaviour>, INotePool
    {
        private readonly Transform _parent;
        private readonly IScoreManager _score;
        private readonly string _catLayerName;

        public NotePool(NoteBehaviour prefab, Transform parent, IScoreManager score, string catLayerName) : base(prefab)
        {
            _parent = parent;
            _score = score;
            _catLayerName = catLayerName;
        }

        public INoteBehaviour Get() => GetPool().Get();

        public void Release(INoteBehaviour note)
        {
            if (note is not NoteBehaviour pooledNote)
                throw new System.ArgumentException("NotePool can release only NoteBehaviour instances.", nameof(note));

            pooledNote.Release();
        }

        protected override NoteBehaviour OnCreate()
        {
            // Parent a new instance under the manager and provide its pool and score manager.
            var note = Object.Instantiate(Prefab, _parent);
            note.Initialize(PoolInstance, _score, _catLayerName);
            note.gameObject.SetActive(false);
            return note;
        }
    }
}
