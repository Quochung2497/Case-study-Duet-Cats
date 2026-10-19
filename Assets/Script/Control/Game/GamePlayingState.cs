using Game;

namespace Control.Game
{
    public class GamePlayingState : State<GameState>
    {
        private readonly IScoreManager _score;
        private readonly ICatBehaviour _leftCat;
        private readonly ICatBehaviour _rightCat;
        private readonly bool _waitForCatActionsOnWin;
        private readonly IGameHost _host;
        private bool _winPending;

        public GamePlayingState(IScoreManager score, ICatBehaviour leftCat, ICatBehaviour rightCat,
            bool waitForCatActionsOnWin, IGameHost host) : base(GameState.Playing)
        {
            _score = score;
            _leftCat = leftCat;
            _rightCat = rightCat;
            _waitForCatActionsOnWin = waitForCatActionsOnWin;
            _host = host;
        }

        public override void EnterState()
        {
            _winPending = false;
            _score.ResetScore();
            _leftCat.StartPlaying();
            _rightCat.StartPlaying();
        }

        public override void UpdateState(float deltaTime)
        {
            // Retry only after song completion has requested a win.
            if (_winPending) TryFinishRun(true);
        }

        public override void ExitState() => _winPending = false;

        public void OnNoteMissed() => TryFinishRun(false);

        public void OnSongFinished() => TryFinishRun(true);

        private void TryFinishRun(bool won)
        {
            // Read readiness through the cat contract without knowing its state or animation clip.
            if (won && _waitForCatActionsOnWin &&
                (!_leftCat.CanTransition() || !_rightCat.CanTransition()))
            {
                _winPending = true;
                return;
            }

            // Clear the request before the host exits Playing to avoid duplicate completion.
            _winPending = false;
            _host.FinishRun(won);
        }
    }
}
