namespace NTC.FiniteStateMachine
{
    public interface IState<out TInitializer>
    {
        public TInitializer Initializer { get; }
        public virtual void OnEnter() { }
        public virtual void OnRun() { }
        public virtual void OnExit() { }
    }
}