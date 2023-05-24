namespace NTC.ContextStateMachine
{
    public interface IStateMachineBased<TInitializer>
    {
        public StateMachine<TInitializer> StateMachine { get; }
    }
}