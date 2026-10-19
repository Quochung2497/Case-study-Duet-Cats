using Game;

namespace Control.Game
{
    public class GameCtaState : State<GameState>
    {
        private readonly ICatBehaviour _leftCat;
        private readonly ICatBehaviour _rightCat;
        private readonly IGameHost _host;
        private bool _reloading;

        public GameCtaState(ICatBehaviour leftCat, ICatBehaviour rightCat, IGameHost host)
            : base(GameState.Cta)
        {
            _leftCat = leftCat;
            _rightCat = rightCat;
            _host = host;
        }

        public override void EnterState()
        {
            _leftCat.SetVisible(false);
            _rightCat.SetVisible(false);
        }

        public void OnPointerDown()
        {
            if (_reloading) return;
            _reloading = true;
            _host.ScheduleReload();
        }
    }
}
