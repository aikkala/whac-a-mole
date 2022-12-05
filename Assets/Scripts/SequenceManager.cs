using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using TMPro.SpriteAssetUtilities;
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
    get
    {
      return _points;
    }
    set
    {
      _points = value;
      pointCounterText.text = _points.ToString();
    }
  }

  private int _maxRounds = 5;
  private int _round;
  public int Round
  {
    get { return _round; }
    set
    {
      _round = value;
      roundCounterText.text = _round.ToString() + " / " + _maxRounds;
    }
  }
  
  // Set round length (in seconds)
  private float _roundLength = 10;
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
  
  public void AddPunchPoints(int punchPoints) {
    Points = _points + punchPoints;
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

    // Increment round, start points from zero
    Round = _round + 1;
    Points = 0;

    // Start round timer
    _roundStart = Time.time;
    
    // Show scoreboard
    // ShowScoreboard(false);
  }
  
  void OnUpdatePlay() 
  {
    // Check if time is up for this trial
    if (Time.time - _roundStart >= _roundLength)
    {
      // Check if game is finished
      if (Round == _maxRounds)
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
    // ShowScoreboard(true);
  }
  
  void OnEnterDone(string text) 
  {
    SpawnFrontText(text);
  }
  
  void OnUpdateDone() 
  {
    stateMachine.GotoNextState();
  }
  
  void ShowScoreboard(bool show) 
  {
    Globals.Instance.scoreboard.GetComponent<ScaleToggle>().Show(show);
  }
  
  void OnEnterReady(string mainText, string buttonText) 
  {
    // Move confirm box to where origin of TargetArea will be
    // Globals.Instance.confirmBox.transform.parent.position = headset.transform.position + targetArea.TargetAreaPosition;

    Globals.Instance.confirmBox.transform.parent.transform.position = headset.position + targetArea.TargetAreaPosition;
    
    // Globals.Instance.debugText.text = "confirm box position: " + Globals.Instance.confirmBox.transform.parent.position.x + ", " 
                                      // + Globals.Instance.confirmBox.transform.parent.position.y + ", " 
                                      // + Globals.Instance.confirmBox.transform.parent.position.z + "\n\n" +
      // "headset position: " + headset.position.x + ", " + headset.position.y + ", " + headset.position.z;

    
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
  }

  void OnUpdateSaving() 
  {
    // if (!Globals.Instance.sensorTracker.saveThreadRunning) 
    // {
      // stateMachine.GotoNextState();
    // }
  }
}
