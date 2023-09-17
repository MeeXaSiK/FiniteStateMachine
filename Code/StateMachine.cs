using System;
using System.Collections.Generic;

namespace NTC.FiniteStateMachine
{
    public class StateMachine<TInitializer>
    {
        private readonly Dictionary<Type, IState<TInitializer>> _states =
            new Dictionary<Type, IState<TInitializer>>(Constants.DefaultCollectionSize);
        
        private readonly List<Transition<TInitializer>> _anyTransitions = 
            new List<Transition<TInitializer>>(Constants.DefaultCollectionSize);
        
        private readonly List<Transition<TInitializer>> _transitions =
            new List<Transition<TInitializer>>(Constants.DefaultCollectionSize);

        public StateMachine()
        {
            
        }

        public StateMachine(params IState<TInitializer>[] states)
        {
            AddStates(states);
        }
        
        public bool TransitionsEnabled { get; set; } = true;
        public bool HasCurrentState { get; private set; }
        public bool HasStatesBeenAdded { get; private set; }
        
        public IState<TInitializer> CurrentState { get; private set; }
        public Transition<TInitializer> CurrentTransition { get; private set; }

        public void AddStates(params IState<TInitializer>[] states)
        {
#if DEBUG
            if (HasStatesBeenAdded)
                throw new Exception("States have already been added!");
            
            if (states.Length == 0)
                throw new Exception("You are trying to add an empty state array!");
#endif
            foreach (var state in states)
            {
                AddState(state);
            }

            HasStatesBeenAdded = true;
        }
        
        public TState GetState<TState>() where TState : IState<TInitializer>
        {
            return (TState) GetState(typeof(TState));
        }

        public void SetState<TState>() where TState : IState<TInitializer>
        {
            SetState(typeof(TState));
        }

        public void AddTransition<TStateFrom, TStateTo>(Func<bool> condition)
            where TStateFrom : IState<TInitializer>
            where TStateTo : IState<TInitializer>
        {
#if DEBUG
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));
#endif
            var stateFrom = GetState(typeof(TStateFrom));
            var stateTo = GetState(typeof(TStateTo));
            
            _transitions.Add(new Transition<TInitializer>(stateFrom, stateTo, condition));
        }

        public void AddAnyTransition<TStateTo>(Func<bool> condition)
            where TStateTo : IState<TInitializer>
        {
#if DEBUG
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));
#endif
            var stateTo = GetState(typeof(TStateTo));

            _anyTransitions.Add(new Transition<TInitializer>(null, stateTo, condition));
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

        private void AddState(IState<TInitializer> state)
        {
#if DEBUG
            if (state == null)
                throw new NullReferenceException(nameof(state));
#endif
            Type stateType = state.GetType();
#if DEBUG
            if (_states.ContainsKey(stateType))
                throw new Exception($"You are trying to add the same state twice! The <{stateType}> already exists!");
#endif
            _states.Add(stateType, state);
        }
        
        private IState<TInitializer> GetState(Type type)
        {
            if (_states.TryGetValue(type, out var state))
            {
                return state;
            }

            throw new Exception($"You didn't add the <{type}> state!");
        }
        
        private void SetState(Type type)
        {
            if (HasCurrentState)
            {
                CurrentState.OnExit();
            }

            CurrentState = GetState(type);
            HasCurrentState = true;
            CurrentState.OnEnter();
        }
        
        private Transition<TInitializer> GetTransition()
        {
            for (var i = 0; i < _anyTransitions.Count; i++)
            {
                if (_anyTransitions[i].Condition.Invoke())
                {
                    return _anyTransitions[i];
                }
            }

            for (var i = 0; i < _transitions.Count; i++)
            {
                if (_transitions[i].From != CurrentState)
                {
                    continue;
                }
                
                if (_transitions[i].Condition.Invoke())
                {
                    return _transitions[i];
                }
            }

            return null;
        }
    }
}