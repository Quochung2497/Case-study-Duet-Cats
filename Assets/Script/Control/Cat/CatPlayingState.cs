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

        // Loop Listening while the song plays and the cat is not being dragged.
        public override void EnterState() => _animation.PlayPlaying();
    }
}
