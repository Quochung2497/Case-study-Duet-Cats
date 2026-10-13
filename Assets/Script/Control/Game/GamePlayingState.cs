using Game;

namespace Control.Game
{
    public class GamePlayingState : State<GameState>
    {
        private readonly IScoreManager _score;
        private readonly ICatBehaviour _leftCat;
        private readonly ICatBehaviour _rightCat;
        private readonly IGameHost _host;

        public GamePlayingState(IScoreManager score, ICatBehaviour leftCat, ICatBehaviour rightCat,
            IGameHost host) : base(GameState.Playing)
        {
            _score = score;
            _leftCat = leftCat;
            _rightCat = rightCat;
            _host = host;
        }

        public override void EnterState()
        {
            _score.ResetScore();
            _leftCat.StartPlaying();
            _rightCat.StartPlaying();
        }

        public void OnNoteMissed() => _host.FinishRun(false);

        public void OnSongFinished() => _host.FinishRun(true);
    }
}
