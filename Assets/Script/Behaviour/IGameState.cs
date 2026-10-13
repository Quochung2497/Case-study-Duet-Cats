using System;
using Control;
using Control.Game;
using Input;

namespace Game
{
    public interface IGameState
    {
        GameState State { get; }
        bool Won { get; }
        event Action<GameState> StateChanged;
        event Action StartTransitionBegan;

        void ConfigureStateMachine(IStateMachine<GameState> fsm, GameStartState startState,
            GamePlayingState playingState, GameCtaState ctaState);
        void Initialize(INoteManager notes, IInputReader input, IGameFlowSettings settings);
    }
}
