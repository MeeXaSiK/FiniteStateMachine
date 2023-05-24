using System;

namespace NTC.ContextStateMachine
{
    public static class StateMachineShortCuts
    {
        public static void SetState<T>(this IStateMachineBased<T> stateMachineBased) where T : State<T>
        {
            stateMachineBased.StateMachine.SetState<T>();
        }

        public static void AddTransition<T>(this IStateMachineBased<T> stateMachineBased, State<T> from, State<T> to, Func<bool> condition)
        {
            stateMachineBased.StateMachine.AddTransition(from, to, condition);
        }
        
        public static void AddAnyTransition<T>(this IStateMachineBased<T> stateMachineBased, State<T> to, Func<bool> condition)
        {
            stateMachineBased.StateMachine.AddAnyTransition(to, condition);
        }
    }
}