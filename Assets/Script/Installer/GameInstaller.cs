using Input;
using UnityEngine;
using Utility.DependencyInjection;

namespace Game
{
    public sealed class GameInstaller : MonoBehaviour, IInstaller
    {
        [Inject] private INoteManager _notes;
        [Inject] private IGameState _manager;
        [Inject] private LayoutSceneRefs _sceneRefs;
        [Inject] private IInputReader _input;
        [Inject] private IGameFlowSettings _settings;
        [Inject] private IScoreManager _score;

        public void AwakeInitialize()
        {
            // Find CatBehaviour on each scene transform and wire both into GameManager.
            if (_notes == null || _manager == null || _sceneRefs == null ||
                _input == null || _settings == null || _score == null ||
                _sceneRefs.LeftCat == null || _sceneRefs.RightCat == null ||
                !_sceneRefs.LeftCat.TryGetComponent<CatBehaviour>(out var leftCat) ||
                !_sceneRefs.RightCat.TryGetComponent<CatBehaviour>(out var rightCat))
            {
                Debug.LogError("GameInstaller needs the note manager, both cats, input, settings, and score manager.", this);
                return;
            }

            _manager.Initialize(_notes, leftCat, rightCat,
                _input, _settings, _score);
        }

        public void StartInitialize() { }

        private void Awake() => AwakeInitialize();
        private void Start() => StartInitialize();
    }
}
