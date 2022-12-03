using System;

namespace NTC.ContextStateMachine
{
    public static class StateMachineShortCuts
    {
        public static void SetState(this IStateMachineBased stateMachineBased, State state)
        {
            stateMachineBased.StateMachine.SetState(state);
        }

        public static void AddTransition(this IStateMachineBased stateMachineBased, State from, State to, Func<bool> condition)
        {
            stateMachineBased.StateMachine.AddTransition(from, to, condition);
        }
        
        public static void AddAnyTransition(this IStateMachineBased stateMachineBased, State to, Func<bool> condition)
        {
            stateMachineBased.StateMachine.AddAnyTransition(to, condition);
        }

        public static void AddListenerOnStateChanged(this IStateMachineBased stateMachineBased, Action<State> action)
        {
            stateMachineBased.StateMachine.OnStateChanged += action;
        }
        
        public static void RemoveListenerOnStateChanged(this IStateMachineBased stateMachineBased, Action<State> action)
        {
            stateMachineBased.StateMachine.OnStateChanged -= action;
        }
    }
}