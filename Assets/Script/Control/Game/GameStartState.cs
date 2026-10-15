using Game;

namespace Control.Game
{
    public class GameStartState : State<GameState>
    {
        private readonly INoteManager _notes;
        private readonly IGameFlowSettings _settings;
        private readonly IGameHost _host;
        private float _remainingSeconds;
        private bool _starting;

        public GameStartState(INoteManager notes, IGameFlowSettings settings, IGameHost host)
            : base(GameState.Start)
        {
            _notes = notes;
            _settings = settings;
            _host = host;
        }

        public void OnPointerDown()
        {
            if (_starting) return;
            _starting = true;
            _remainingSeconds = _settings.StartTransitionSeconds;
            _host.BeginStartTransition();
        }

        public override void UpdateState(float deltaTime)
        {
            if (!_starting) return;
            _remainingSeconds -= deltaTime;
            if (_remainingSeconds > 0f) return;
            _starting = false;

            // Stay in Start and restore its UI if the song could not be scheduled.
            if (!_notes.StartSong())
            {
                _host.RefreshCurrentState();
                return;
            }

            _host.TransitionTo(GameState.Playing);
        }
    }
}
