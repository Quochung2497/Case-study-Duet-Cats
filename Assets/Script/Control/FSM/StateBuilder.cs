using System;
using System.Collections.Generic;

namespace Control
{
    public class StateBuilder<TState> where TState : Enum
    {
        private readonly List<IState<TState>> _states = new();

        public StateBuilder<TState> Add(IState<TState> state)
        {
            _states.Add(state ?? throw new ArgumentNullException(nameof(state)));
            return this;
        }

        public IStateMachine<TState> Build(TState initialState)
        {
            return new StateManager<TState>(initialState, _states.ToArray());
        }
    }
}
