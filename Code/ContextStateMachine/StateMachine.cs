using System;
using System.Collections.Generic;

namespace NTC.ContextStateMachine
{
    public class StateMachine<TInitializer>
    {
        public bool TransitionsEnabled { get; set; } = true;
        public bool HasCurrentState { get; private set; }
        
        public State<TInitializer> CurrentState { get; private set; }
        public Transition<TInitializer> CurrentTransition { get; private set; }

        private readonly HashSet<State<TInitializer>> _states = new(16);
        private readonly List<Transition<TInitializer>> _anyTransitions = new(16);
        private readonly List<Transition<TInitializer>> _transitions = new(16);

        private bool _isStatesAdded;

        public StateMachine(params State<TInitializer>[] states)
        {
            AddStates(states);
        }

        public void AddStates(params State<TInitializer>[] states)
        {
            if (_isStatesAdded)
            {
                throw new Exception("States already added!");
            }
            
            foreach (var state in states)
            {
                if (state == null)
                {
                    throw new NullReferenceException(nameof(state));
                }
                
                _states.Add(state);
            }

            if (states.Length > 0)
            {
                _isStatesAdded = true;
            }
        }

        public void SetState<TState>() where TState : State<TInitializer>
        {
            SetState(typeof(TState));
        }
        
        public void AddTransition<TStateFrom, TStateTo>(Func<bool> condition)
            where TStateFrom : State<TInitializer>
            where TStateTo : State<TInitializer>
        {
            var stateFrom = GetState(typeof(TStateFrom));
            var stateTo = GetState(typeof(TStateTo));
            
            _transitions.Add(new Transition<TInitializer>(stateFrom, stateTo, condition));
        }

        public void AddAnyTransition<TStateTo>(Func<bool> condition)
            where TStateTo : State<TInitializer>
        {
            var stateTo = GetState(typeof(TStateTo));

            _anyTransitions.Add(new Transition<TInitializer>(null, stateTo, condition));
        }
        
        public void Run()
        {
            if (TransitionsEnabled)
            {
                SetStateByTransitions();
            }

            if (HasCurrentState)
            {
                CurrentState.OnRun();
            }
        }

        public void SetStateByTransitions()
        {
            CurrentTransition = GetTransition();

            if (CurrentTransition == null)
                return;
            
            if (CurrentState == CurrentTransition.To)
                return;
            
            SetState(CurrentTransition.To.GetType());
        }

        public TState GetState<TState>() where TState : State<TInitializer>
        {
            return (TState) GetState(typeof(TState));
        }

        private State<TInitializer> GetState(Type type)
        {
            foreach (var state in _states)
            {
                if (state.GetType() == type)
                {
                    return state;
                }
            }

            throw new Exception($"The <{type.Name}> is not found!");
        }
        
        private void SetState(Type type)
        {
            if (HasCurrentState)
            {
                ExitCurrentState();
            }

            CurrentState = GetState(type);
            HasCurrentState = true;
            
            EnterCurrentState();
        }

        private void EnterCurrentState()
        {
            CurrentState.IsActive = true;
            CurrentState.OnEnter();
        }

        private void ExitCurrentState()
        {
            CurrentState.IsActive = false;
            CurrentState.OnExit();
        }

        private Transition<TInitializer> GetTransition()
        {
            for (var i = 0; i < _anyTransitions.Count; i++)
            {
                if (_anyTransitions[i].Condition())
                {
                    return _anyTransitions[i];
                }
            }

            for (var i = 0; i < _transitions.Count; i++)
            {
                if (_transitions[i].From.IsActive == false)
                {
                    continue;
                }
                
                if (_transitions[i].Condition())
                {
                    return _transitions[i];
                }
            }

            return default;
        }
    }
}