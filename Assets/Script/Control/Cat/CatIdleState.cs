using Control;

namespace Control.Cat
{
    public class CatIdleState : State<CatState>
    {
        public CatIdleState() : base(CatState.Idle) { }
    }
}
