# 🚄 Finite State Machine
[![License](https://img.shields.io/github/license/meexasik/nightpool?color=318CE7&style=flat-square)](LICENSE.md) [![Version](https://img.shields.io/github/package-json/v/MeeXaSiK/FiniteStateMachine?color=318CE7&style=flat-square)](package.json) [![Unity](https://img.shields.io/badge/Unity-2021.3+-2296F3.svg?color=318CE7&style=flat-square)](https://unity.com/)

This is a lightweight **Finite State Machine** for your projects based on C# 8+
* ▶️ High performance
* ▶️ Supports transitions
* ▶️ Abstracted from the game engine

# 🌐 Navigation

* [Main](#-finite-state-machine)
* [Installation](#-installation)
* [How to use](#-how-to-use)
  * [Implementing StateMachine](#1-implement-statemachinetinitializer-field-or-property)
  * [Creating some states](#2-create-some-states)
  * [Installing states in your class](#3-install-states-in-your-class)
  * [Adding states to the StateMachine](#4-add-states-to-the-statemachine)
  * [Binding transitions for states](#5-bind-transitions-for-states)
  * [Launching the StateMachine](#6-launch-the-statemachine)
  * [Result](#what-should-be-the-result)
  * [Disable Transitions](#disable-transitions)
  
# ▶ Installation

## As a Unity module
Supports installation as a Unity module via a git link in the **PackageManager**
```
https://github.com/MeeXaSiK/FiniteStateMachine.git
```
or direct editing of `Packages/manifest.json`:
```
"com.nighttraincode.fsm": "https://github.com/MeeXaSiK/FiniteStateMachine.git",
```
## As source
You can also clone the code into your Unity project.

# 🔸 How to use

### 1. Implement `StateMachine<TInitializer>` field or property

`TInitializer` is the class to which the states will belong. 

The namespace you need is `using NTC.FiniteStateMachine`.

```csharp
public readonly StateMachine<Sample> StateMachine = new StateMachine<Sample>();
```
```csharp
public StateMachine<Sample> StateMachine { get; } = new StateMachine<Sample>();
```

### 2. Create some states

Create some states for an entity and declare the methods you need: `OnEnter()`, `OnRun()`, `OnExit()`.

| Method | Info |
| ------ | ---- |
| `OnEnter()` | Called once upon entering the state. |
| `OnRun()` | Execution is determined by update methods. |
| `OnExit()` | Called once when exiting the state. |

For example, let's create two states:

The `AwaitingState` is necessary for an entity to wait for some action.
You also need to implement the `TInitializer` property. 
Let me remind you that the `TInitializer` is the class to which the state belongs.
In this case, the initializer is `Sample`:

```csharp
public class AwaitingState : IState<Sample>
{
    private readonly IFollower _follower;
    
    public AwaitingState(IFollower follower, Sample sample)
    {
        _follower = follower;
        Initializer = sample;
    }
    
    public Sample Initializer { get; }

    public void OnEnter()
    {
        _follower.StopFollow();
    }
}
```

The `FollowingState` is necessary for the entity to follow a target:

```csharp
public class FollowingState : IState<Sample>
{
    private readonly IFollower _follower;
    private readonly Func<Transform> _getTarget;
        
    public FollowingState(IFollower follower, Func<Transform> getTarget, Sample sample)
    {
        _follower = follower;
        _getTarget = getTarget;
        Initializer = sample;
    }

    public Sample Initializer { get; }

    public void OnRun()
    {
        Transform target = _getTarget.Invoke();

        if (target != null)
        {
            _follower.Follow(target.position);
        }
    }
}
```

### 3. Install states in your class

This **Finite State Machine** is abstracted from any game engine, but I'll show you how it works with an example in Unity:

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
        _followingState = new FollowingState(follower, GetTarget, this);
    }

    private Transform GetTarget()
    {
        return Target;
    }
}
```

### 4. Add states to the StateMachine

You can add states using the `AddStates` method or the class constructor.

```csharp
StateMachine.AddStates(_awaitingState, _followingState);
```
```csharp
StateMachine = new StateMachine<Sample>(_awaitingState, _followingState);
```

> **Warning!** You can only add states to the `StateMachine` once!

### 5. Bind transitions for states

We have methods for binding transitions such as `AddTransition` and `AddAnyTransition`.

| Method | Info |
| ------ | ---- |
| `AddTransition<TStateFrom, TStateTo>(Func<bool> condition)` | Takes two states as arguments, from which state we want to go to the second state and the transition condition. |
| `AddAnyTransition<TStateTo>(Func<bool> condition)` | Takes as arguments the one state we want to switch to and the transition condition. |

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
using NTC.FiniteStateMachine;
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
    
    private void Update()
    {
        StateMachine.Run();
    }
    
    private void InstallStates()
    {
        var follower = GetComponent<IFollower>();
    
        _awaitingState = new AwaitingState(follower, this);
        _followingState = new FollowingState(follower, GetTarget, this);
    
        StateMachine.AddStates(_awaitingState, _followingState);
    }

    private Transform GetTarget()
    {
        return Target;
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
