using Game;

namespace Control.Game
{
    public class GameResultState : State<GameState>
    {
        private readonly INoteManager _notes;
        private readonly ICatBehaviour _leftCat;
        private readonly ICatBehaviour _rightCat;
        private readonly IGameFlowSettings _settings;
        private readonly IGameHost _host;
        private float _remainingSeconds;

        public GameResultState(INoteManager notes, ICatBehaviour leftCat, ICatBehaviour rightCat,
            IGameFlowSettings settings, IGameHost host) : base(GameState.Result)
        {
            _notes = notes;
            _leftCat = leftCat;
            _rightCat = rightCat;
            _settings = settings;
            _host = host;
        }

        public override void EnterState()
        {
            _remainingSeconds = _settings.ResultSeconds;
            _notes.StopSong();
            _leftCat.ShowResult(_host.Won);
            _rightCat.ShowResult(_host.Won);
        }

        public override void UpdateState(float deltaTime)
        {
            _remainingSeconds -= deltaTime;
            if (_remainingSeconds <= 0f)
                _host.TransitionTo(GameState.Cta);
        }
    }
}
