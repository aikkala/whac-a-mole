using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum TargetState {
  Alive       = 0,
  Dead        = 1,
}

public class Target: MonoBehaviour {
  
  // State Machine
  public StateMachine<TargetState> stateMachine;
  
  // Target position and size
  public Vector3 Position
  {
    get => transform.localPosition;
    set
    {
      transform.localPosition = value;
    }
  }

  private float _originalScale;
  public float Size
  {
    get => transform.localScale.x;
    set
    {
      _originalScale = value;
      transform.localScale = new Vector3(value, value, value);
      // Set position again, so that the face of the target is on the plane
      transform.localPosition += Vector3.forward * value;
    }
  }

  // Target life span; calculate time-of-death when life span is set
  private float _lifeSpan;
  public float LifeSpan
  {
    get => _lifeSpan;
    set
    {
      _lifeSpan = value;
      _tod = _spawnTime + _lifeSpan;
    }
  }
  
  // Spawn time and time-of-death
  private float _spawnTime, _tod;

  // Color of target, changes based on how much time until tod
  private Color _colorStart, _colorEnd;
  private Material _material;
  
  // Target fades in (wrt to size) when alive
  private float _fadeInTime = 0.1f;
  
  // Target fades away once dead
  private float _fadeOutTime = 0.2f;

  public virtual void Awake()
  {

    // If lookahead is enabled, initialise the targets in TargetState.LookAhead
    stateMachine = new StateMachine<TargetState>(TargetState.Alive);

    stateMachine.AddTransition(TargetState.Alive,     TargetState.Dead);

    stateMachine.State(TargetState.Alive)     .OnFixedUpdate += OnFixedUpdateAlive;

    stateMachine.State(TargetState.Alive)     .OnUpdate      += OnUpdateAlive;
    stateMachine.State(TargetState.Dead)      .OnUpdate      += OnUpdateDead;
    
    stateMachine.State(TargetState.Alive)     .OnCollision   += TargetCollision;
    
    Globals.Instance.sequenceManager          .PlayStop      += DestroyTarget;
    
    // Get spawn time
    _spawnTime = Time.fixedTime;
    
    // Color changes from green to red depending on how much lifespan is left
    _colorStart = Color.green;
    _colorEnd = Color.red;
    _material = gameObject.GetComponentInChildren<MeshRenderer>().material;
    _material.color = _colorStart;
  }
  
  private void OnTriggerEnter(Collider other) {
    stateMachine.CurrentState().InvokeOnCollision(other);
  }

  void Update() {
    stateMachine.CurrentState().InvokeOnUpdate();
  }

  private void FixedUpdate()
  {
    stateMachine.CurrentState().InvokeOnFixedUpdate();
  }
  
  void DestroyTarget() {
    Globals.Instance.sequenceManager.PlayStop -= DestroyTarget;
    Destroy(gameObject);
  }

  void OnFixedUpdateAlive() {
    
    // If time has expired, make this box dead (and cannot be punched anymore)
    if (Time.fixedTime >= _tod)
    {
      _material.color = Color.black;
      stateMachine.GotoState(TargetState.Dead);
      Globals.Instance.sequenceManager.RecordMiss();
    }
  }

  void OnUpdateAlive() {
    // Do a fade in with size
    var elapsed = Time.time - _spawnTime;

    // Fade in
    float scale = Math.Min(1.0f, elapsed / _fadeInTime);
    transform.localScale = scale*_originalScale*Vector3.one;
    
    // Change color according to how much time this target as left; color ranges from green to red
    _material.color = Color.Lerp(_colorStart, _colorEnd, (Time.time - _spawnTime) / _lifeSpan);
  }
  
  private void TargetCollision(Collider other) {

    // Collision counts as a hit only if the relative velocity is high enough (punch is strong enough)
    if (other.GetComponent<ObjectMovement>().Velocity.z < Globals.Instance.punchVelocityThreshold)
    {
       return;
    }
    
    // Update time-of-death
    _tod = Time.fixedTime;
      
    // Change target color to blue to indicate it has been punched
    _material.color = Color.blue;

    // Record punch
    Globals.Instance.sequenceManager.RecordPunch();
    
    // Move to dead state
    stateMachine.GotoState(TargetState.Dead);
  }
  
  // private void OnEnterPunched() 
  // {
  //   // Change target color to blue to indicate it has been punched
  //   _material.color = Color.blue;
  //   
  //   // Move to next state
  //   stateMachine.GotoNextState();
  // }

  // private void OnEnterMissed()
  // {
  //   // Change target color to black to indicate it has been missed
  //   _material.color = Color.black;
  //   
  //   // Move to next state
  //   stateMachine.GotoNextState();
  // }

  private void OnUpdateDead()
  {
    // The target fades away (quickly)
    var elapsed = Time.time - _tod;
    if (elapsed <= _fadeOutTime)
    {
      // Fade away
      float scale = Math.Max(0.0f, 1 - elapsed / _fadeOutTime);
      transform.localScale = scale*_originalScale*Vector3.one;
    }
    else
    {
      // After fading away, destroy target
      DestroyTarget();
    }
  }
  
}

 