# Context State Machine
Lightweight Context State Machine for Unity
* Supports condition transitions
* Can be used for any entity
* Awesome performance

## Navigation

* [Installation](#installation)
* [How to use](#how-to-use)
  * [Implementing StateMachine](#1-implement-statemachinetinitializer-field-or-property)
  * [Creating some states](#2-create-some-states)
  * [Installing states in your class](#3-install-states-in-your-class)
  * [Adding states to the StateMachine](#4-add-states-to-the-statemachine)
  * [Binding transitions for states](#5-bind-transitions-for-states)
  * [Launching the StateMachine](#6-launch-the-statemachine)
  * [Result](#what-should-be-the-result)
  * [Disable Transitions](#disable-transitions)
  
## Installation

Add files in your Unity project.

## How to use

### 1. Implement `StateMachine<TInitializer>` field or property

`TInitializer` is the class to which the states will belong

```csharp
public readonly StateMachine<Sample> StateMachine = new StateMachine<Sample>();
```
```csharp
public StateMachine<Sample> StateMachine { get; } = new StateMachine<Sample>();
```

### 2. Create some states

Create some states for an entity. In states you can override methods you need: `OnEnter()`, `OnRun()`, `OnExit()`.

| Method | Info |
| ------ | ---- |
| `OnEnter()` | Called once upon entering the state |
| `OnRun()` | Execution is determined by update methods |
| `OnExit()` | Called once when exiting the state |

For example, let's create two states:

`AwaitingState` is necessary to wait for the entity of some condition

You also need to pass an `TInitializer` to the base State constructor. In this case, the initializer is the `Sample`

```csharp
public class AwaitingState : State<Sample>
{
    private IFollower Follower { get; }
    
    public AwaitingState(IFollower follower, Sample sample) : base(sample)
    {
        Follower = follower;
    }
        
    public override void OnEnter()
    {
        Follower.StopFollow();
    }
}
```

`FollowingState` is necessary for the entity to follow some target

```csharp
public class FollowingState : State<Sample>
{
    private IFollower Follower { get; }
    private Transform Target { get; }
        
    public FollowingState(IFollower follower, Transform target, Sample sample) : base(sample)
    {
        Follower = follower;
        Target = target;
    }

    public override void OnRun()
    {
        Follower.Follow(Target);
    }
}
```

### 3. Install states in your class

```csharp
[RequireComponent(typeof(IFollower))]
public class Sample : MonoBehaviour
{
    [SerializeField] private Health health;
    
    public StateMachine<Sample> StateMachine { get; } = new StateMachine<Sample>();
    public Transform Target { get; set; }

    private AwaitingState _awaitingState;
    private FollowingState _followingState;
    
    private void Awake()
    {
        InstallStates();
    }

    private void InstallStates()
    {
        var follower = GetComponent<IFollower>();

        _awaitingState = new AwaitingState(follower, this);
        _followingState = new FollowingState(follower, Target, this);
    }
}
```

### 4. Add states to the StateMachine

You can add states with a method or a constructor

```csharp
StateMachine.AddStates(_awaitingState, _followingState);
```
```csharp
StateMachine = new StateMachine<Sample>(_awaitingState, _followingState);
```

You can only add states to the StateMachine once!

### 5. Bind transitions for states

We have methods for binding transitions such as `AddTransition` and `AddAnyTransition`.

| Method | Info |
| ------ | ---- |
| `AddTransition<TStateFrom, TStateTo>(Func<bool> condition)` | Takes two states as arguments, from which state we want to go to the second state and the transition condition |
| `AddAnyTransition<TStateTo>(Func<bool> condition)` | Takes as arguments the one state we want to switch to and the transition condition |

Let's imagine a situation:

We want to switch from the `AwatingState` to the `FollowingState` if a target is found. We also want to switch back to the `AwaitingState` if the target is lost. `AddTransition` method will help us with this, let's add two transitions:

From `AwaitingState` to `FollowingState`

```csharp
StateMachine.AddTransition<AwaitingState, FollowingState>(condition: () => Target != null);
```

From `FollowingState` to `AwaitingState`

```csharp
StateMachine.AddTransition<FollowingState, AwaitingState>(condition: () => Target == null);
```

We will also add a transition to the `AwaitingState` from any state.
Why might this be needed? If the entity is not alive, then it is logical that it will not be able to move and will have to switch to the idle state.

Add transition from any state to idle `AwaitingState`

```csharp
StateMachine.AddAnyTransition<AwaitingState>(condition: () => health.IsAlive == false);
```

### 6. Launch the StateMachine

For the launch of `StateMachine` you need to set first state and call the `Run()` method in `Update()`:

```csharp

private void Start()
{
    StateMachine.SetState<AwaitingState>();
}

private void Update()
{
    StateMachine.Run();
}
```

### What should be the result

```csharp
using NTC.ContextStateMachine;
using UnityEngine;

[RequireComponent(typeof(IFollower))]
public class Sample : MonoBehaviour
{
    [SerializeField] private Health health;

    public StateMachine<Sample> StateMachine { get; } = new StateMachine<Sample>();
    public Transform Target { get; set; }

    private AwaitingState _awaitingState;
    private FollowingState _followingState;
    
    private void Awake()
    {
        InstallStates();
        
        BindTransitions();
        
        BindAnyTransitions();
        
        StateMachine.SetState<AwaitingState>();
    }
    
    private void InstallStates()
    {
        var follower = GetComponent<IFollower>();
    
        _awaitingState = new AwaitingState(follower, this);
        _followingState = new FollowingState(follower, Target, this);
    
        StateMachine.AddStates(_awaitingState, _followingState);
    }
    
    private void BindTransitions()
    {
        StateMachine.AddTransition<AwaitingState, FollowingState>(condition: () => Target != null);
        StateMachine.AddTransition<FollowingState, AwaitingState>(condition: () => Target == null);
    }
    
    private void BindAnyTransitions()
    {
        StateMachine.AddAnyTransition<AwaitingState>(condition: () => health.IsAlive == false);
    }
    
    private void Update()
    {
        StateMachine.Run();
    }
}
```

### Disable Transitions

If you want to set states manually, you can disable `TransitionsEnabled` in the `StateMachine`:

```csharp
StateMachine.TransitionsEnabled = false;
```

Then you can change state of `StateMachine` by method `SetState<TState>()`:

```csharp
StateMachine.SetState<TState>();
```

Also if you don't want the `StateMachine` to choose the state in the update method, you can use the method `SetStateByTransitions()` when you need:

```csharp
StateMachine.SetStateByTransitions();
```
