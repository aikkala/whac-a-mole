using System;
using UnityEngine;

public enum TargetState {
  FadeIn      = 0,
  Alive       = 1,
  FadeOut     = 2,
}

public class Target : MonoBehaviour
{

  // State Machine
  public StateMachine<TargetState> stateMachine;

  // Punch velocity threshold
  private float punchVelocityThreshold = 0.6f;

  // ID
  public int ID { get; set; }

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
      _tod = _spawnTime + _lifeSpan + _fadeInTime;
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
    stateMachine = new StateMachine<TargetState>(TargetState.FadeIn);

    stateMachine.AddTransition(TargetState.FadeIn,     TargetState.Alive);
    stateMachine.AddTransition(TargetState.Alive,     TargetState.FadeOut);

    stateMachine.State(TargetState.Alive)     .OnFixedUpdate += OnFixedUpdateAlive;
    stateMachine.State(TargetState.Alive)     .OnUpdate      += OnUpdateAlive;

    stateMachine.State(TargetState.FadeIn)    .OnUpdate      += OnUpdateFadeIn;
    stateMachine.State(TargetState.FadeOut)   .OnUpdate      += OnUpdateFadeOut;

    stateMachine.State(TargetState.FadeIn)    .OnExit        += OnExitFadeIn;

    Globals.Instance.sequenceManager          .PlayStop      += DestroyTarget;
    
    // Get spawn time
    _spawnTime = Time.fixedTime;
    
    // Color changes from green to red depending on how much lifespan is left
    _colorStart = Color.green;
    _colorEnd = Color.red;
    _material = gameObject.GetComponentInChildren<MeshRenderer>().material;
    _material.color = _colorStart;
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
    
    // If time has expired, move to FadeOut
    if (Time.fixedTime >= _tod)
    {
      _material.color = Color.black;
      Globals.Instance.sequenceManager.RecordMiss(this);
      stateMachine.GotoState(TargetState.FadeOut);
    }
  }

  void OnUpdateFadeIn() {
    // Do a fade in with size
    var elapsed = Time.time - _spawnTime;

    if (elapsed <= _fadeInTime)
    {
      // Fade in
      float scale = Math.Min(1.0f, elapsed / _fadeInTime);
      transform.localScale = scale * _originalScale * Vector3.one;
    }
    else
    {
      // Move to Alive state
      stateMachine.GotoState(TargetState.Alive);
    }
  }

  void OnExitFadeIn()
  {
    // Make sure size is correct
    transform.localScale = _originalScale * Vector3.one;
  }

  void OnUpdateAlive()
  {
    // Change color according to how much time this target as left; color ranges from green to red
    _material.color = Color.Lerp(_colorStart, _colorEnd, (Time.time - (_spawnTime+_fadeInTime)) / _lifeSpan);
  }
  
  void OnUpdateFadeOut()
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
  
  private void OnTriggerEnter(Collider other) {
    
    // Target can be hit only when it is Alive
    if (stateMachine.currentState != TargetState.Alive)
    {
      return;
    }
    
    // Collision counts as a hit only if the relative velocity is high enough (punch is strong enough)
    Vector3 velocity = other.GetComponent<ObjectMovement>().Velocity;
    // if (velocity.z < punchVelocityThreshold || velocity.y > -punchVelocityThreshold)
    if (velocity.z < punchVelocityThreshold)
    {
      return;
    }
  
    // Update time-of-death
    _tod = Time.fixedTime;
      
    // Change target color to blue to indicate it has been punched
    _material.color = Color.blue;
  
    // Record punch
    Globals.Instance.sequenceManager.RecordPunch(this);
    
    // Move to FadeOut
    stateMachine.GotoState(TargetState.FadeOut);
  }
  
  public string PositionToString()
  {
    Vector3 pos = transform.position;
    return pos.x + ", " + pos.y + ", " + pos.z;
  }
  
}

 