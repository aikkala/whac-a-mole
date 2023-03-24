using System;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR.Interaction.Toolkit;

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
  public Func<Vector3, bool> VelocityThreshold { get; set; }

  // Points for hitting
  public int points;
  
  // ID
  public int ID { get; set; }
  
  // Grid position
  public Tuple<int, int> GridPosition { get; set; }
  public int GridID { get; set; }
  
  // Score text
  private TextMeshProUGUI _scoreText;

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
  private float _fadeOutTime = 0.3f;
  
  // Particle effect
  private ParticleSystem _particleSystem;

  // Target mesh
  private GameObject _mesh;

  // Haptics
  public bool hapticsEnabled;
  private ActionBasedController _xr;

  public virtual void Awake()
  {
    stateMachine = new StateMachine<TargetState>(TargetState.FadeIn);

    stateMachine.AddTransition(TargetState.FadeIn,     TargetState.Alive);
    stateMachine.AddTransition(TargetState.Alive,      TargetState.FadeOut);

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
    
    // Score is not displayed initially
    _scoreText = transform.Find("score_canvas/score_text").GetComponent<TextMeshProUGUI>();
    _scoreText.enabled = false;
    
    // Get particle system
    _particleSystem = transform.Find("explosion").GetComponent<ParticleSystem>();
    _particleSystem.Stop();

    // Target mesh
    _mesh = transform.Find("mesh").gameObject;

    // Get controller
    _xr = GameObject.Find("RightHand Controller").GetComponent<ActionBasedController>();
  }

  public void SetPosition(Tuple<int, int> idx, Tuple<float, float> pos, int gridID)
  {
    GridPosition = idx;
    GridID = gridID;
    Position = new Vector3(pos.Item1, pos.Item2, Size);
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
    if (_mesh.activeSelf && elapsed <= _fadeOutTime)
    {
      // Fade away
      float scale = Math.Max(0.0f, 1 - elapsed / _fadeOutTime);
      transform.localScale = scale*_originalScale*Vector3.one;
    }
    else if (elapsed > _fadeOutTime)
    {
      // After fading away, free up this position in the grid and destroy target
      Globals.Instance.sequenceManager.targetArea.RemoveTarget(this);
      DestroyTarget();
    }
  }

  private void OnTriggerEnter(Collider other)
  {

    // Target can be hit only when it is Alive
    if (stateMachine.currentState != TargetState.Alive)
    {
      return;
    }

    // Collision counts as a hit only if the relative velocity is high enough (punch is strong enough)
    Vector3 velocity = other.GetComponent<ObjectMovement>().Velocity;
    if (!VelocityThreshold(velocity))
    {
      // Record contact -- but no hit!
      Globals.Instance.sequenceManager.RecordContact(this, velocity);
      return;
    }
    Hit(velocity);
  }

  public void Hit()
  {
    Hit(new Vector3(0, 0, 0));
  }
  public void Hit(Vector3 velocity) {

    // Target can be hit only when it is Alive
    if (stateMachine.currentState != TargetState.Alive)
    {
      return;
    }

    // Update time-of-death
    _tod = Time.fixedTime;
    
    // Record punch
    Globals.Instance.sequenceManager.RecordHit(this, velocity);
    
    // Activate particle effect
    _particleSystem.Play();
    
    // Hide mesh
    _mesh.SetActive(false);

    // Haptic feedback
    if (hapticsEnabled)
    {
      _xr.SendHapticImpulse(0.2f, 0.05f);
    }
    
    // Move to FadeOut
    stateMachine.GotoState(TargetState.FadeOut);
  }
  
  public string PositionToString(string delimiter=", ")
  {
    Vector3 pos = transform.position;
    return pos.x + delimiter + pos.y + delimiter + pos.z;
  }
}

 