using System;

namespace Control
{
  public interface IStateMachine<TState> where TState : Enum
  {
    TState CurrentStateKey { get; }
    void TransitionToState(TState nextStateKey);
    void Tick(float deltaTime);
  }
}