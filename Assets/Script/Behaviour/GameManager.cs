using System;
using System.Collections;
using Control;
using Control.Game;
using Input;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public enum GameState
    {
        Start,
        Playing,
        Result,
        Cta
    }

    public class GameManager : MonoBehaviour, IGameState, IGameHost
    {
#if UNITY_EDITOR
        [Header("Debug")]
        [Tooltip("Ignore all misses so the song can finish with a win in the Editor.")]
        [SerializeField] private bool ignoreMisses;
#endif

        private INoteManager _notes;
        private IInputReader _input;
        private IGameFlowSettings _settings;
        private IStateMachine<GameState> _fsm;
        private GameStartState _startState;
        private GamePlayingState _playingState;
        private GameCtaState _ctaState;

        public GameState State => _fsm?.CurrentStateKey ?? GameState.Start;
        public bool Won { get; private set; }
        public event Action<GameState> StateChanged;
        public event Action StartTransitionBegan;

        public void ConfigureStateMachine(IStateMachine<GameState> fsm, GameStartState startState,
            GamePlayingState playingState, GameCtaState ctaState)
        {
            _fsm = fsm;
            _startState = startState;
            _playingState = playingState;
            _ctaState = ctaState;
        }

        public void Initialize(INoteManager notes, IInputReader input, IGameFlowSettings settings)
        {
            _notes = notes;
            _input = input;
            _settings = settings;

            _input.PointerDown += OnPointerDown;
            _notes.NoteMissed += OnNoteMissed;
            _notes.SongFinished += OnSongFinished;
        }

        private void Start() => StateChanged?.Invoke(State);

        private void Update() => _fsm?.Tick(Time.unscaledDeltaTime);

        private void OnDestroy()
        {
            if (_input != null) _input.PointerDown -= OnPointerDown;
            if (_notes == null) return;
            _notes.NoteMissed -= OnNoteMissed;
            _notes.SongFinished -= OnSongFinished;
        }

        private void OnPointerDown(PointerSample _)
        {
            if (State == GameState.Start)
                _startState.OnPointerDown();
            else if (State == GameState.Cta)
                _ctaState.OnPointerDown();
        }

        private void OnNoteMissed()
        {
#if UNITY_EDITOR
            if (ignoreMisses) return;
#endif
            if (State == GameState.Playing) _playingState.OnNoteMissed();
        }

        private void OnSongFinished()
        {
            if (State == GameState.Playing) _playingState.OnSongFinished();
        }

        void IGameHost.TransitionTo(GameState nextState) => ChangeState(nextState);

        private void ChangeState(GameState nextState)
        {
            if (State == nextState) return;
            _fsm.TransitionToState(nextState);
            StateChanged?.Invoke(State);
        }

        void IGameHost.FinishRun(bool won)
        {
            if (State != GameState.Playing) return;
            Won = won;
            ChangeState(GameState.Result);
        }

        void IGameHost.BeginStartTransition() => StartTransitionBegan?.Invoke();

        void IGameHost.RefreshCurrentState() => StateChanged?.Invoke(State);

        void IGameHost.ScheduleReload() => StartCoroutine(ReloadSceneAfterDelay());

        private IEnumerator ReloadSceneAfterDelay()
        {
            yield return new WaitForSecondsRealtime(_settings.CtaReloadSeconds);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
