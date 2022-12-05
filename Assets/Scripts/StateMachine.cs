using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public delegate void StateExitEvent();
public delegate void StateEnterEvent();

public delegate void StateUpdateEvent();
public delegate void StateFixedUpdateEvent();
public delegate void StateCollisionEvent(Collider other);

public class StateEventHandler {
  private const string OnUpdateKey      = "onUpdate";
  private const string OnFixedUpdateKey = "onFixedUpdate";
  private const string OnExitKey        = "onExit";
  private const string OnEnterKey       = "onEnter";
  private const string OnCollisionKey   = "onCollision";

  private readonly Dictionary<string, System.Delegate> handlers;

  public StateEventHandler() {
    handlers = new Dictionary<string, System.Delegate> {
      [OnUpdateKey]      = null,
      [OnFixedUpdateKey] = null,
      [OnExitKey]        = null,
      [OnEnterKey]       = null,
      [OnCollisionKey]   = null,
    };
  }

  public event StateUpdateEvent OnUpdate {
    add {
      handlers[OnUpdateKey] = (StateUpdateEvent) handlers[OnUpdateKey] + value;
    }
    remove {
      handlers[OnUpdateKey] = (StateUpdateEvent) handlers[OnUpdateKey] - value;
    }
  }

  public event StateFixedUpdateEvent OnFixedUpdate {
    add {
      handlers[OnFixedUpdateKey] = (StateFixedUpdateEvent) handlers[OnFixedUpdateKey] + value;
    }
    remove {
      handlers[OnFixedUpdateKey] = (StateFixedUpdateEvent) handlers[OnFixedUpdateKey] - value;
    }
  }

  public event StateExitEvent OnExit {
    add {
      handlers[OnExitKey] = (StateExitEvent) handlers[OnExitKey] + value;
    }
    remove {
      handlers[OnExitKey] = (StateExitEvent) handlers[OnExitKey] - value;
    }
  }

  public event StateEnterEvent OnEnter {
    add {
      handlers[OnEnterKey] = (StateEnterEvent) handlers[OnEnterKey] + value;
    }
    remove {
      handlers[OnEnterKey] = (StateEnterEvent) handlers[OnEnterKey] - value;
    }
  }
  
  public event StateCollisionEvent OnCollision {
    add {
      handlers[OnCollisionKey] = (StateCollisionEvent) handlers[OnCollisionKey] + value;
    }
    remove {
      handlers[OnCollisionKey] = (StateCollisionEvent) handlers[OnCollisionKey] - value;
    }
  }

  internal void InvokeOnUpdate() {
    ((StateUpdateEvent) handlers[OnUpdateKey])?.Invoke();
  }

  internal void InvokeOnFixedUpdate()
  {
    ((StateFixedUpdateEvent) handlers[OnFixedUpdateKey])?.Invoke();
  }

  internal void InvokeOnExit() {
    ((StateExitEvent) handlers[OnExitKey])?.Invoke();
  }

  internal void InvokeOnEnter() {
    ((StateEnterEvent) handlers[OnEnterKey])?.Invoke();
  }

  internal void InvokeOnCollision(Collider other) {
    ((StateCollisionEvent) handlers[OnCollisionKey])?.Invoke(other);
  }
}

public class StateMachine<TState> where TState : System.Enum {
  public TState currentState;
  public Dictionary<TState, TState> defaultTransitions;

  public Dictionary<TState, StateEventHandler> stateHandlers;
  
  public System.Action<TState> AnyStateEnter;
  public System.Action<TState> AnyStateUpdate;
  public System.Action<TState> AnyStateFixedUpdate;
  public System.Action<TState> AnyStateExit;

  public StateEventHandler State(TState state) {
    if (!stateHandlers.ContainsKey(state)) {
      stateHandlers.Add(state, new StateEventHandler());
      State(state).OnEnter       +=   ( ) => { if (AnyStateEnter != null) AnyStateEnter(state); };
      State(state).OnUpdate      +=   ( ) => { if (AnyStateUpdate != null) AnyStateUpdate(state); };
      State(state).OnFixedUpdate +=   ( ) => { if (AnyStateFixedUpdate != null) AnyStateFixedUpdate(state); };
      State(state).OnExit        +=   ( ) => { if (AnyStateExit != null) AnyStateExit(state); };
    }

    return stateHandlers[state];
  }

  public StateEventHandler CurrentState() {
    return State(currentState);
  }

  public void AddTransition(TState from, TState to) {
    if (defaultTransitions.ContainsKey(from))
      defaultTransitions[from] = to;
    else 
      defaultTransitions.Add(from, to);
  }

  public StateMachine(TState def) {
    currentState = def;
    defaultTransitions = new Dictionary<TState, TState>();
    
    stateHandlers = new Dictionary<TState, StateEventHandler>();
  }

  public TState GotoNextState() {
    if (defaultTransitions.ContainsKey(currentState)) {
      GotoState(defaultTransitions[currentState]);
    } else {
      Debug.Log("No next state for " + currentState.ToString("g"));
    }

    return currentState;
  }

  public TState GotoState(TState newState) {
    State(currentState).InvokeOnExit();
    currentState = newState;
    State(currentState).InvokeOnEnter();
    
    return currentState;
  }

}