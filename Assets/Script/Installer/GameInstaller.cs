using Input;
using UnityEngine;
using Utility.DependencyInjection;

namespace Game
{
    public sealed class GameInstaller : MonoBehaviour, IInstaller
    {
        [Inject] private NoteManager _notes;
        [Inject] private GameManager _manager;
        [Inject] private LayoutSceneRefs _sceneRefs;
        [Inject] private IInputReader _input;
        [Inject] private PlayableSettings _settings;

        public void AwakeInitialize()
        {
            if (_notes == null || _manager == null || _sceneRefs == null ||
                _input == null || _settings == null ||
                _sceneRefs.LeftCat == null || _sceneRefs.RightCat == null ||
                !_sceneRefs.LeftCat.TryGetComponent(out CatBehaviour leftCat) ||
                !_sceneRefs.RightCat.TryGetComponent(out CatBehaviour rightCat))
            {
                Debug.LogError("GameInstaller needs the note manager and both cats.", this);
                return;
            }

            _manager.Initialize(_notes, leftCat, rightCat,
                _input, _settings);
        }

        public void StartInitialize() { }

        private void Awake() => AwakeInitialize();
        private void Start() => StartInitialize();
    }
}
