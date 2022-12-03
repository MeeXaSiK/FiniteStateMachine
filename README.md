# Context State Machine
Lightweight Context State Machine for Unity
* Optionally selects states automatically
* Can be used for any entity
* Awesome performance

## Navigation

* [Installation](#installation)
* [How to use](#how-to-use)
  * [How to implement StateMachine](#1-implement-statemachine-field-or-property)
  * [How to create some states](#2-how-to-create-some-states)
  * [How to install states in your class](#3-how-to-install-states-in-your-class)
  * [How to bind transitions for states](#4-how-to-bind-transitions-for-states)
  * [How to launch the StateMachine](#5-how-to-launch-the-statemachine)
  * [Result](#what-should-be-the-result)
  * [Manual state selection](#manual-state-selection)
  
## Installation

Add files in your Unity project.

## How to use

### 1. Implement `StateMachine` field or property

```csharp
public readonly StateMachine StateMachine = new StateMachine();
```
```csharp
public StateMachine StateMachine { get; } = new StateMachine();
```

### 2. How to create some states

Create some states for an entity. In states you can override methods you need: `OnEnter()`, `OnRun()`, `OnExit()`.

| Method | Info |
| ------ | ---- |
| `OnEnter()` | Called once upon entering the state |
| `OnRun()` | Execution is determined by update methods |
| `OnExit()` | Called once when exiting the state |

For example, let's create two states:

`AwaitingState` is necessary to wait for the entity of some condition

```csharp
public class AwaitingState : State
{
    private IFollower Follower { get; }
        
    public AwaitingState(IFollower follower)
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
public class FollowingState : State
{
    private IFollower Follower { get; }
    private Transform Target { get; }
        
    public FollowingState(IFollower follower, Transform target)
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

### 3. How to install states in your class

```csharp
[RequireComponent(typeof(IFollower))]
public class Sample : MonoBehaviour
{
    [SerializeField] private Health health;
    
    public StateMachine StateMachine { get; } = new StateMachine();
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

        _awaitingState = new AwaitingState(follower);
        _followingState = new FollowingState(follower, Target);
    }
}
```

### 4. How to bind transitions for states

We have methods for binding transitions such as `AddTransition` and `AddAnyTransition`.

| Method | Info |
| ------ | ---- |
| `AddTransition(from, to, condition)` | Takes two states as arguments, from which state we want to go to the second state and the transition condition |
| `AddAnyTransition(to, condition)` | Takes as arguments the one state we want to switch to and the transition condition |

Let's imagine a situation:

We want to switch from the `AwatingState` to the `FollowingState` if a target is found. We also want to switch back to the `AwaitingState` if the target is lost. `AddTransition` method will help us with this, let's add two transitions:

From `AwaitingState` to `FollowingState`

```csharp
StateMachine.AddTransition(from: _awaitingState, to: _followingState, condition: () => Target != null);
```

From `FollowingState` to `AwaitingState`

```csharp
StateMachine.AddTransition(from: _followingState, to: _awaitingState, condition: () => Target == null);
```

We will also add a transition to the `AwaitingState` from any state.
Why might this be needed? If the entity is not alive, then it is logical that it will not be able to move and will have to switch to the idle state.

Add transition from any state to idle `AwaitingState`

```csharp
StateMachine.AddAnyTransition(to: _awaitingState, condition: () => health.IsAlive == false);
```

### 5. How to launch the StateMachine

For the launch of `StateMachine` you need to call the `Run()` method in `Update()`:

```csharp
private void Update()
{
    StateMachine.Run();
}
```

### What should be the result

```csharp
[RequireComponent(typeof(IFollower))]
public class Sample : MonoBehaviour
{
    [SerializeField] private Health health;
    
    public StateMachine StateMachine { get; } = new StateMachine();
    public Transform Target { get; set; }

    private AwaitingState _awaitingState;
    private FollowingState _followingState;
    
    private void Awake()
    {
        InstallStates();
        
        BindTransitions();
        
        BindAnyTransitions();
    }

    private void InstallStates()
    {
        var follower = GetComponent<IFollower>();

        _awaitingState = new AwaitingState(follower);
        _followingState = new FollowingState(follower, Target);
    }

    private void BindTransitions()
    {
        StateMachine.AddTransition(from: _awaitingState, to: _followingState, condition: () => Target != null);
        StateMachine.AddTransition(from: _followingState, to: _awaitingState, condition: () => Target == null);
    }
    
    private void BindAnyTransitions()
    {
        StateMachine.AddAnyTransition(to: _awaitingState, condition: () => health.IsAlive == false);
    }
    
    private void Update()
    {
        StateMachine.Run();
    }
}
```

### Manual state selection

If you want to set states manually, you can disable `AutoSelectState` in the `StateMachine`:

```csharp
StateMachine.AutoSelectState = false;
```

```csharp
public StateMachine StateMachine { get; } = new StateMachine(autoSelectState: false);
```

Then you can change state of `StateMachine` by method `SetState()`:

```csharp
StateMachine.SetState(newState);
```
