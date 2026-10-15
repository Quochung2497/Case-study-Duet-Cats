using System;
using System.Collections.Generic;

namespace Control
{
  public class StateManager<TState> : IStateMachine<TState> where TState : Enum
  {
    // The dictionary finds a destination state by its enum key.
    private readonly Dictionary<TState, IState<TState>> _states;
    private IState<TState> _currentState;

    public TState CurrentStateKey => _currentState.StateKey;
    public bool CanTransition() => _currentState.CanTransition();

    public StateManager(TState initialStateKey, params IState<TState>[] states)
    {
      if (states == null)
        throw new ArgumentNullException(nameof(states));

      _states = new Dictionary<TState, IState<TState>>();
      // Reject duplicate keys while building the FSM so transitions stay unambiguous.
      foreach (var state in states)
      {
        if (state == null)
          throw new ArgumentException("A state cannot be null.", nameof(states));
        if (_states.ContainsKey(state.StateKey))
          throw new ArgumentException("Each state key must be unique.", nameof(states));

        _states.Add(state.StateKey, state);
      }

      if (!_states.TryGetValue(initialStateKey, out _currentState))
        throw new ArgumentException("The initial state must be registered.", nameof(initialStateKey));

      foreach (var state in _states.Values)
        state.SetStateMachine(this);

      // Enter the initial state as soon as the state machine is built.
      _currentState.EnterState();
    }

    public void Tick(float deltaTime)
    {
      _currentState.UpdateState(deltaTime);
    }

    public void TransitionToState(TState nextStateKey)
    {
      if (!_states.TryGetValue(nextStateKey, out var nextState))
        throw new ArgumentException("The destination state must be registered.", nameof(nextStateKey));
      // Transitioning to the current state does not call Exit or Enter again.
      if (ReferenceEquals(_currentState, nextState))
        return;

      // A transition exits the old state and enters the new one immediately.
      _currentState.ExitState();
      _currentState = nextState;
      _currentState.EnterState();
    }
  }
}
