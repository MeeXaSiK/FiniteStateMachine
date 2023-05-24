namespace NTC.ContextStateMachine
{
    public abstract class State<TInitializer>
    {
        public TInitializer Initializer { get; }
        public bool IsActive { get; set; }

        public virtual void OnEnter() { }
        public virtual void OnRun() { }
        public virtual void OnExit() { }
    }
}