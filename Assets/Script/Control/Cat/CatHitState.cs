using Control;
using Control.Note;

namespace Control.Cat
{
    public class CatHitState : State<CatState>
    {
        private readonly ICatAnimation _animation;
        private NoteVisualType _type;
        private bool _complete;

        public bool IsComplete => _complete;

        public CatHitState(ICatAnimation animation) : base(CatState.Hit)
        {
            _animation = animation;
        }

        public void OnHit(NoteVisualType type)
        {
            _type = type;
            if (StateMachine != null && StateMachine.CurrentStateKey == CatState.Hit)
                PlayHit();
        }

        public override void EnterState() => PlayHit();

        private void PlayHit()
        {
            _complete = false;
            _animation.PlayHit(_type, () => _complete = true);
        }
    }
}
