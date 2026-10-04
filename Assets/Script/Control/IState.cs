using System;

namespace DanielTran.Control
{
  public interface IState<TState> where TState : Enum
  {
    TState StateKey { get; }
    void SetStateMachine(IStateMachine<TState> stateMachine);
    void EnterState();
    void UpdateState(float deltaTime);
    void ExitState();
  }
}