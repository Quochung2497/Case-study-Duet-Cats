using Control;

namespace Control.Cat
{
    public class CatIdleState : State<CatState>
    {
        private readonly ICatAnimation _animation;
        private bool _complete;

        public CatIdleState(ICatAnimation animation) : base(CatState.Idle)
        {
            _animation = animation;
        }

        public override void EnterState() => PlayNext();

        public override void UpdateState(float deltaTime)
        {
            // Choose the next idle clip when one ends instead of looping a single clip.
            if (_complete) PlayNext();
        }

        private void PlayNext()
        {
            _complete = false;
            _animation.PlayIdle(() => _complete = true);
        }
    }
}
