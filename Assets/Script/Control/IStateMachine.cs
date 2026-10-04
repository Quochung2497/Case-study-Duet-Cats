using System;

namespace DanielTran.Control
{
  public interface IStateMachine<TState> where TState : Enum
  {
    TState CurrentStateKey { get; }
    void TransitionToState(TState nextStateKey);
    void Tick(float deltaTime);
  }
}