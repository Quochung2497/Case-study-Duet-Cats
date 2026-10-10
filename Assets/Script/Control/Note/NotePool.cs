using UnityEngine;
using Utility;

namespace Game
{
    public class NotePool : Pool<NoteBehaviour>
    {
        private readonly Transform _parent;

        public NotePool(NoteBehaviour prefab, Transform parent) : base(prefab)
        {
            _parent = parent;
        }

        protected override NoteBehaviour OnCreate()
        {
            // Parent a new instance under the manager and tell it which pool to return to.
            var note = Object.Instantiate(Prefab, _parent);
            note.SetPool(PoolInstance);
            note.gameObject.SetActive(false);
            return note;
        }
    }
}
