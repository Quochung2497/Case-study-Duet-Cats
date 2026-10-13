using Game;

namespace Control.Game
{
    // Game states request transitions and scene-level events through their host.
    public interface IGameHost
    {
        bool Won { get; }
        void TransitionTo(GameState nextState);
        void FinishRun(bool won);
        void BeginStartTransition();
        void RefreshCurrentState();
        void ScheduleReload();
    }
}
