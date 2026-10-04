using Control;

namespace Control.Cat
{
    public class CatTrackingState : State<CatState>
    {
        private readonly ICatAction _action;

        public CatTrackingState(ICatAction action) : base(CatState.Tracking)
        {
            _action = action;
        }

        public override void UpdateState(float deltaTime)
        {
            if (!_action.IsDragging)
                StateMachine.TransitionToState(CatState.Idle);
        }
    }
}
