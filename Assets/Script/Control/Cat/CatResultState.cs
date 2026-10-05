using Control;

namespace Control.Cat
{
    public class CatResultState : State<CatState>
    {
        private readonly ICatAnimation _animation;
        private bool _won;

        public CatResultState(ICatAnimation animation) : base(CatState.Result)
        {
            _animation = animation;
        }

        public void SetOutcome(bool won) => _won = won;

        public override void EnterState() => _animation.PlayResult(_won);
    }
}
