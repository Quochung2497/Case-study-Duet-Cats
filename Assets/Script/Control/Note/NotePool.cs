using UnityEngine;
using Utility;

namespace Game
{
    public class NotePool : Pool<NoteBehaviour>
    {
        private readonly Transform _parent;
        private readonly IScoreManager _score;

        public NotePool(NoteBehaviour prefab, Transform parent, IScoreManager score) : base(prefab)
        {
            _parent = parent;
            _score = score;
        }

        protected override NoteBehaviour OnCreate()
        {
            // Parent a new instance under the manager and provide its pool and score manager.
            var note = Object.Instantiate(Prefab, _parent);
            note.Initialize(PoolInstance, _score);
            note.gameObject.SetActive(false);
            return note;
        }
    }
}
