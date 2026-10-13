using Control;
using Control.Game;
using Input;
using UnityEngine;
using Utility.DependencyInjection;

namespace Game
{
    public sealed class GameInstaller : MonoBehaviour, IInstaller
    {
        [Inject] private INoteManager _notes;
        [Inject] private IGameState _manager;
        [Inject] private IGameHost _host;
        [Inject] private LayoutSceneRefs _sceneRefs;
        [Inject] private IInputReader _input;
        [Inject] private IGameFlowSettings _settings;
        [Inject] private IScoreManager _score;

        public void AwakeInitialize()
        {
            // Find the scene cats before building the game FSM.
            if (_notes == null || _manager == null || _host == null || _sceneRefs == null ||
                _input == null || _settings == null || _score == null ||
                _sceneRefs.LeftCat == null || _sceneRefs.RightCat == null ||
                !_sceneRefs.LeftCat.TryGetComponent<CatBehaviour>(out var leftCat) ||
                !_sceneRefs.RightCat.TryGetComponent<CatBehaviour>(out var rightCat))
            {
                Debug.LogError("GameInstaller needs the note manager, both cats, input, settings, and score manager.", this);
                return;
            }

            var startState = new GameStartState(_notes, _settings, _host);
            var playingState = new GamePlayingState(_score, leftCat, rightCat, _host);
            var ctaState = new GameCtaState(leftCat, rightCat, _host);
            var fsm = new StateBuilder<GameState>()
                .Add(startState)
                .Add(playingState)
                .Add(new GameResultState(_notes, leftCat, rightCat, _settings, _host))
                .Add(ctaState)
                .Build(GameState.Start);

            _manager.ConfigureStateMachine(fsm, startState, playingState, ctaState);
            _manager.Initialize(_notes, _input, _settings);
        }

        public void StartInitialize() { }

        private void Awake() => AwakeInitialize();
        private void Start() => StartInitialize();
    }
}
