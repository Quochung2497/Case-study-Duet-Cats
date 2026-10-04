using System;
using System.Collections.Generic;

namespace Control
{
  public class StateManager<TState> : IStateMachine<TState> where TState : Enum
  {
    private readonly Dictionary<TState, IState<TState>> _states;
    private IState<TState> _currentState;

    public TState CurrentStateKey => _currentState.StateKey;

    public StateManager(TState initialStateKey, params IState<TState>[] states)
    {
      if (states == null)
        throw new ArgumentNullException(nameof(states));

      _states = new Dictionary<TState, IState<TState>>();
      foreach (IState<TState> state in states)
      {
        if (state == null)
          throw new ArgumentException("A state cannot be null.", nameof(states));
        if (_states.ContainsKey(state.StateKey))
          throw new ArgumentException("Each state key must be unique.", nameof(states));

        _states.Add(state.StateKey, state);
      }

      if (!_states.TryGetValue(initialStateKey, out _currentState))
        throw new ArgumentException("The initial state must be registered.", nameof(initialStateKey));

      foreach (IState<TState> state in _states.Values)
        state.SetStateMachine(this);

      _currentState.EnterState();
    }

    public void Tick(float deltaTime)
    {
      _currentState.UpdateState(deltaTime);
    }

    public void TransitionToState(TState nextStateKey)
    {
      if (!_states.TryGetValue(nextStateKey, out IState<TState> nextState))
        throw new ArgumentException("The destination state must be registered.", nameof(nextStateKey));
      if (ReferenceEquals(_currentState, nextState))
        return;

      _currentState.ExitState();
      _currentState = nextState;
      _currentState.EnterState();
    }
  }
}