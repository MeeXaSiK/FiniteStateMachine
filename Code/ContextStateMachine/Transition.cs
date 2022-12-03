using System;

namespace NTC.ContextStateMachine
{
    public class Transition
    {
        public Func<bool> Condition { get; }
        public State To { get; }
        public State From { get; }

        public Transition(State from, State to, Func<bool> condition)
        {
            From = from;
            To = to;
            Condition = condition;
        }
    }
}