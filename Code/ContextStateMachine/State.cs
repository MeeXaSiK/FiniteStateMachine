namespace NTC.ContextStateMachine
{
    public abstract class State
    {
        public virtual void OnEnter() { }
        public virtual void OnRun() { }
        public virtual void OnExit() { }
    }
}