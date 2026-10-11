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

        void Initialize(INoteManager notes, CatBehaviour leftCat, CatBehaviour rightCat,
            IInputReader input, IGameFlowSettings settings, IScoreManager score);
    }
}
