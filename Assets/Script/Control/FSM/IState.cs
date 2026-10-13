using System;

namespace Control
{
  // A state has a key and enter, update, and exit phases.
  public interface IState<TState> where TState : Enum
  {
    TState StateKey { get; }
    void SetStateMachine(IStateMachine<TState> stateMachine);
    void EnterState();
    void UpdateState(float deltaTime);
    void ExitState();
    bool CanTransition();
  }
}
