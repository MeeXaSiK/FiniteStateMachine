namespace NTC.FiniteStateMachine
{
    public interface IState<out TInitializer>
    {
        public TInitializer Initializer { get; }
        public void OnEnter() { }
        public void OnRun() { }
        public void OnExit() { }
    }
}