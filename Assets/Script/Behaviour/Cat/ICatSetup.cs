using Control;
using Control.Cat;
using Input;

namespace Game
{
    public interface ICatSetup
    {
        void ConfigureStateMachine(IStateMachine<CatState> fsm, CatHitState hitState,
            CatResultState resultState);
        void Initialize(ICatAction action, bool left, IInputReader input, IResponsiveLayout layout);
    }
}
