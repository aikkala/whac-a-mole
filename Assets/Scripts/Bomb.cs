using System;
using UnityEngine;

public enum BombState {
  FadeIn      = 0,
  Alive       = 1,
  FadeOut     = 2,
}

public class Bomb : MonoBehaviour
{

  // State Machine
  public StateMachine<BombState> stateMachine;

  // Punch velocity threshold
  private float punchVelocityThreshold = 0.6f;

  // ID
  public int ID { get; set; }

  // Bomb position and size
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

  // Bomb life span; calculate time-of-death when life span is set
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
  
  // Color of bomb
  private Color _color;
  private Material _material;
  
  // Bomb fades in (wrt to size) when alive
  private float _fadeInTime = 0.1f;
  
  // Bomb fades away once dead
  private float _fadeOutTime = 0.2f;

  public virtual void Awake()
  {

    // If lookahead is enabled, initialise the targets in TargetState.LookAhead
    stateMachine = new StateMachine<BombState>(BombState.FadeIn);

    stateMachine.AddTransition(BombState.FadeIn,    BombState.Alive);
    stateMachine.AddTransition(BombState.Alive,     BombState.FadeOut);

    stateMachine.State(BombState.Alive)     .OnFixedUpdate += OnFixedUpdateAlive;

    stateMachine.State(BombState.FadeIn)    .OnUpdate      += OnUpdateFadeIn;
    stateMachine.State(BombState.FadeOut)   .OnUpdate      += OnUpdateFadeOut;

    stateMachine.State(BombState.FadeIn)    .OnExit        += OnExitFadeIn;

    Globals.Instance.sequenceManager        .PlayStop      += DestroyBomb;
    
    // Get spawn time
    _spawnTime = Time.fixedTime;
    
    _material = gameObject.GetComponentInChildren<MeshRenderer>().material;
    _material.color = Color.black;
  }
  
  void Update() {
    stateMachine.CurrentState().InvokeOnUpdate();
  }

  private void FixedUpdate()
  {
    stateMachine.CurrentState().InvokeOnFixedUpdate();
  }
  
  void DestroyBomb() {
    Globals.Instance.sequenceManager.PlayStop -= DestroyBomb;
    Destroy(gameObject);
  }

  void OnFixedUpdateAlive() {
    
    // If time has expired, move to FadeOut
    if (Time.fixedTime >= _tod)
    {
      Globals.Instance.sequenceManager.RecordBombDisarm(ID, PositionToString());
      stateMachine.GotoState(BombState.FadeOut);
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
      stateMachine.GotoState(BombState.Alive);
    }
  }

  void OnExitFadeIn()
  {
    // Make sure size is correct
    transform.localScale = _originalScale * Vector3.one;
  }
  
  void OnUpdateFadeOut()
  {
    // The bomb fades away (quickly)
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
      DestroyBomb();
    }
  }
  
  private void OnTriggerEnter(Collider other) {
    
    // Bomb can be hit only when it is Alive
    if (stateMachine.currentState != BombState.Alive)
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
    
    // Record punch
    Globals.Instance.sequenceManager.RecordBombDetonation(ID, PositionToString());
    
    // Move to FadeOut
    stateMachine.GotoState(BombState.FadeOut);
  }
  
  public string PositionToString()
  {
    return Position.x + ", " + Position.y + ", " + Position.z;
  }
  
}

 