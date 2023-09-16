using System;

namespace NTC.FiniteStateMachine
{
    public class Transition<TInitializer>
    {
        public readonly IState<TInitializer> From;
        public readonly IState<TInitializer> To;
        public readonly Func<bool> Condition;

        public Transition(IState<TInitializer> from, IState<TInitializer> to, Func<bool> condition)
        {
            From = from;
            To = to;
            Condition = condition;
        }
    }
}