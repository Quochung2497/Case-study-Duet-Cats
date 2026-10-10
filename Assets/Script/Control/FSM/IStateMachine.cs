using System;

namespace Control
{
  // Callers can tick and transition without depending on the concrete StateManager.
  public interface IStateMachine<TState> where TState : Enum
  {
    TState CurrentStateKey { get; }
    void TransitionToState(TState nextStateKey);
    void Tick(float deltaTime);
  }
}
