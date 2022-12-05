namespace NTC.ContextStateMachine
{
    public abstract class State
    {
        public bool IsActive { get; set; }

        public virtual void OnEnter() { }
        public virtual void OnRun() { }
        public virtual void OnExit() { }
    }
}