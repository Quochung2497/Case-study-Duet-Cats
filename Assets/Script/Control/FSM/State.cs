using System;

namespace Control
{
  public abstract class State<TState> : IState<TState> where TState : Enum
  {
    public TState StateKey { get; }
    protected IStateMachine<TState> StateMachine { get; private set; }

    protected State(TState stateKey)
    {
      StateKey = stateKey;
    }

    public void SetStateMachine(IStateMachine<TState> stateMachine)
    {
      if (stateMachine == null)
        throw new ArgumentNullException(nameof(stateMachine));
      if (StateMachine != null)
        throw new InvalidOperationException("This state already belongs to a state machine.");

      StateMachine = stateMachine;
    }

    public virtual void EnterState() { }
    public virtual void UpdateState(float deltaTime) { }
    public virtual void ExitState() { }
  }
}