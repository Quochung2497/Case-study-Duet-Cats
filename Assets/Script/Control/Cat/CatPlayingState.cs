using Control;

namespace Control.Cat
{
    public class CatPlayingState : State<CatState>
    {
        private readonly ICatAnimation _animation;

        public CatPlayingState(ICatAnimation animation) : base(CatState.Playing)
        {
            _animation = animation;
        }

        public override void EnterState() => _animation.PlayPlaying();
    }
}
