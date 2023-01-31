using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using TMPro.SpriteAssetUtilities;
using Unity.XR.CoreUtils;
using UnityEngine;
using UserInTheBox;

public enum GameState {
  Startup        = 0,
  Done           = 1,
  Saving         = 2,
  
  PlayRandom     = 10,
  PlayEasy       = 11,
  PlayMedium     = 12,
  PlayHard       = 13,
  
  Ready          = 20,
}

public class SequenceManager : MonoBehaviour {
  
  // State machine
  public StateMachine<GameState> stateMachine;

  // Headset transform for setting game height
  public Transform headset;
  
  // Set some info texts
  public Transform pointScreen;
  public TextMeshPro pointCounterText;
  public TextMeshPro roundCounterText;
  public GameObject frontTextPrefab;
  public Transform  frontTextAnchor;
  private FrontText _currentFrontText;
  public event System.Action FrontTextShrink;
  
  // Global events
  // public event System.Action<string> UpdateTrackers;
  // public event System.Action<GameState> StateEnter;
  public event System.Action PlayStop;
  // public event System.Action UpdateTargets;
  
  // Game State (for all types of states)
  private RunIdentification currentRunId;
  private float cooldownTimer;
  
  private int _points;
  public int Points
  {
    get => _points;
    private set
    {
      _points = value;
      pointCounterText.text = _points.ToString();
    }
  }

  private const int MaxRounds = 10;
  private int _round;
  private int Round
  {
    get => _round;
    set
    {
      _round = value;
      // roundCounterText.text = _round.ToString() + " / " + MaxRounds;
    }
  }

  // Number of points per punched target
  private const int PunchValue = 1;
  
  // Set max number of total targets (punched and missed) per round
  // private const int MaxTargets = 10;
  private int _punches;
  private int _misses;
  
  // Set length of round (in seconds)
  private const float RoundLength = 10;
  private float _roundStart;
  
  // Target area where targets are spawned
  public TargetArea targetArea;
  
  // State getters
  public RunIdentification CurrentRunIdentification { get {return currentRunId; }}
  
  // Run State
  void InitStateMachine() {
    stateMachine  = new StateMachine<GameState>(GameState.Startup);
    
    // State transitions
    stateMachine.AddTransition(GameState.Startup,         GameState.Ready);
    stateMachine.AddTransition(GameState.Ready,           GameState.PlayRandom);
    stateMachine.AddTransition(GameState.PlayRandom,      GameState.Ready);
    stateMachine.AddTransition(GameState.PlayRandom,      GameState.Done);
    
    stateMachine.AddTransition(GameState.Done,            GameState.Saving);
    stateMachine.AddTransition(GameState.Saving,          GameState.Startup);
    
    // On Enter
    stateMachine.State(GameState.Startup)                 .OnEnter += () => InitRun();
    
    stateMachine.State(GameState.PlayRandom)              .OnEnter += () => OnEnterPlay("random");
    stateMachine.State(GameState.PlayEasy)                .OnEnter += () => OnEnterPlay("easy");
    stateMachine.State(GameState.PlayMedium)              .OnEnter += () => OnEnterPlay("medium");
    stateMachine.State(GameState.PlayHard)                .OnEnter += () => OnEnterPlay("hard");
    
    stateMachine.State(GameState.Ready)                   .OnEnter += () => OnEnterReady("Punch the target to start next round", "");
    
    stateMachine.State(GameState.Done)                    .OnEnter += () => OnEnterDone("Thank you for playing!\n\n Please take off the headset.");
    stateMachine.State(GameState.Saving)                  .OnEnter += () => OnEnterSaving();

    // On Update
    stateMachine.State(GameState.Startup)                 .OnUpdate += () => stateMachine.GotoNextState(); // Allow one update so headset position is updated
    
    stateMachine.State(GameState.PlayRandom)              .OnUpdate += () => OnUpdatePlay();
    stateMachine.State(GameState.PlayEasy)                .OnUpdate += () => OnUpdatePlay();
    stateMachine.State(GameState.PlayMedium)              .OnUpdate += () => OnUpdatePlay();
    stateMachine.State(GameState.PlayHard)                .OnUpdate += () => OnUpdatePlay();
    
    stateMachine.State(GameState.Done)                    .OnUpdate += () => OnUpdateDone();
    stateMachine.State(GameState.Saving)                  .OnUpdate += () => OnUpdateSaving();
    
    // On Exit
    stateMachine.State(GameState.PlayRandom)              .OnExit += () => OnExitPlay();
    stateMachine.State(GameState.PlayEasy)                .OnExit += () => OnExitPlay();
    stateMachine.State(GameState.PlayMedium)              .OnExit += () => OnExitPlay();
    stateMachine.State(GameState.PlayHard)                .OnExit += () => OnExitPlay();
    
    stateMachine.State(GameState.Ready)                   .OnExit += () => OnExitReady();

    stateMachine.State(GameState.Done)                    .OnExit += () => OnExitDone();
  }

  void Awake()
  {
    // Set target frame rate to 60 Hz. This will be overwritten by SimulatedUser during simulations
    Application.targetFrameRate = 60;
  }
  
  void Start() {
    Globals.Instance.confirmBox.triggered += () => stateMachine.GotoNextState();

    // Initialise state machine
    InitStateMachine();
    stateMachine.State(stateMachine.currentState).InvokeOnEnter();
  }
  
  void FixedUpdate()
  {
    stateMachine.CurrentState().InvokeOnFixedUpdate();
  }
  
  void Update() {
    stateMachine.CurrentState().InvokeOnUpdate();
  }
  
  public void RecordPunch() {
    Points = _points + PunchValue;
    _punches += 1;
  }

  public void RecordMiss()
  {
    _misses += 1;
  }
  
  void InitRun() {
    var uid = System.Guid.NewGuid().ToString();
    Debug.Log("Initializing new run with Run ID: " + uid);

    Points = 0;
    Round = 0;

    currentRunId = new RunIdentification();
    currentRunId.uuid = uid;
    currentRunId.startWallTime = System.DateTime.Now.ToString(Globals.Instance.timeFormat);
    currentRunId.startRealTime = Time.realtimeSinceStartup;
  }

  void SpawnFrontText(string text)
  {
    var newFrontText = Instantiate(
      frontTextPrefab,
      frontTextAnchor
    );
    _currentFrontText = newFrontText.GetComponent<FrontText>();
    _currentFrontText.GetComponent<FrontText>().text.text = text;

  }

  public void HideFrontText()
  {
    if (FrontTextShrink != null) FrontTextShrink();
  }
  
  void OnEnterPlay(string difficulty) 
  {
    // Set play (difficulty) parameters
    targetArea.SetLevel(difficulty);

    // Set to correct position
    targetArea.SetPosition(headset);
    
    // Set also position of point screen (showing timer/score)
    Vector3 pointScreenOffset = new Vector3(-0.5f, 0.0f, 2.5f);
    pointScreen.SetPositionAndRotation(headset.position + pointScreenOffset, Quaternion.identity);

    // Increment round, start points from zero
    Round = _round + 1;
    Points = 0;
    _punches = 0;
    _misses = 0;
    _roundStart = Time.time;
    
    // Show scoreboard
    // ShowScoreboard(true);
  }
  
  void OnUpdatePlay() 
  {
    // Update timer
    float elapsed = Time.time - _roundStart;
    roundCounterText.text = (elapsed >= RoundLength ? 0 : RoundLength - elapsed).ToString("N1");
    
    // Check if time is up for this trial
    // if (_punches+_misses >= MaxTargets)
    if (Time.time - _roundStart > RoundLength)
    {
      // Check if game is finished
      if (Round >= MaxRounds)
      {
        stateMachine.GotoState(GameState.Done);
      }
      else
      {
        stateMachine.GotoState(GameState.Ready);
      }
    }
    else
    {
      // Continue play; potentially spawn a new target
      targetArea.SpawnTarget();
    }
  }

  void OnExitPlay()
  {
    if (PlayStop != null) PlayStop();
    // ShowScoreboard(false);
  }
  
  void OnEnterDone(string text) 
  {
    SpawnFrontText(text);
  }

  void OnExitDone()
  {
    HideFrontText();
  }
  
  void OnUpdateDone() 
  {
    stateMachine.GotoNextState();
  }
  
  // void ShowScoreboard(bool show) 
  // {
  //   Globals.Instance.scoreboard.GetComponent<ScaleToggle>().Show(show);
  // }
  
  void OnEnterReady(string mainText, string buttonText) 
  {
    // Move confirm box to the right of target area
    Vector3 offset = new Vector3(-0.2f, -0.2f, 0.0f);
    Globals.Instance.confirmBox.transform.parent.transform.position = headset.position + targetArea.TargetAreaPosition + offset;
    // Initialise confirm box
    SpawnFrontText(mainText);
    Globals.Instance.confirmBox.Show(true, buttonText);
  }
  
  void OnExitReady() 
  { 
    HideFrontText();
    Globals.Instance.confirmBox.Show(false, "");
  }
  
  void OnEnterSaving() 
  {
    // Globals.Instance.sensorTracker.SaveData();
    // Globals.Instance.SetLoadingState(true);
    stateMachine.GotoNextState();
  }

  void OnUpdateSaving() 
  {
    // if (!Globals.Instance.sensorTracker.saveThreadRunning) 
    // {
      // stateMachine.GotoNextState();
    // }
  }
}
