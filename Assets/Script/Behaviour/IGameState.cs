using System;
using Input;

namespace Game
{
    public interface IGameState
    {
        GameState State { get; }
        bool Won { get; }
        event Action<GameState> StateChanged;
        event Action StartTransitionBegan;

        void Initialize(INoteManager notes, ICatBehaviour leftCat, ICatBehaviour rightCat,
            IInputReader input, IGameFlowSettings settings, IScoreManager score);
    }
}
