using System;
using CsvHelper.Configuration.Attributes;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UserInTheBox;

public enum GameState {
  Startup        = 0,
  Ready          = 1,
  Countdown      = 2,
  Play           = 3,
}

public class SequenceManager : MonoBehaviour {
  
  // State machine
  public StateMachine<GameState> stateMachine;

  // Headset transform for setting game height
  public Transform headset;
  
  // Right controller for visualising controller ray / hammer
  public GameObject rightController;
  private XRInteractorLineVisual _lineVisual;
  private GameObject _hammer;
  
  // Set some info texts
  public Transform pointScreen;
  public TextMeshPro pointCounterText;
  public TextMeshPro roundCounterText;
  public event System.Action PlayStop;
  
  // UI for choosing game levels
  public GameObject levelChooser;
  
  // Game State (for all types of states)
  private RunIdentification currentRunId;
  
  // Needed for countdown
  private float _countdownStart;
  private const float CountdownDuration = 4;
  public TextMeshPro countdownText;
  
  // Keep score of points
  private int _points;
  public int Points
  {
    get => _points;
    private set
    {
      _points = value;
    }
  }

  // Number of points per punched target
  private const int PunchValue = 1;
  
  // Set max number of total targets (punched and missed) per round
  private int _punches;
  private int _misses;
  
  // Set length of round (in seconds)
  private const float RoundLength = 10;
  private float _roundStart;
  
  // Boolean to indicate whether episode should be terminated
  private bool _terminate;
  
  // Play parameters
  public PlayParameters playParameters;
  
  // Logger
  public UserInTheBox.Logger logger;
  
  // Target area where targets are spawned
  public TargetArea targetArea;
  
  // State getters
  public RunIdentification CurrentRunIdentification { get {return currentRunId; }}
  
  // Run State
  void InitStateMachine() {
    stateMachine  = new StateMachine<GameState>(GameState.Startup);
    
    // State transitions
    stateMachine.AddTransition(GameState.Startup,         GameState.Ready);
    stateMachine.AddTransition(GameState.Ready,           GameState.Countdown);
    stateMachine.AddTransition(GameState.Countdown,       GameState.Play);
    
    // On Enter
    stateMachine.State(GameState.Startup)                 .OnEnter += () => InitRun();
    stateMachine.State(GameState.Ready)                   .OnEnter += () => OnEnterReady();
    stateMachine.State(GameState.Countdown)               .OnEnter += () => OnEnterCountdown();
    stateMachine.State(GameState.Play)                    .OnEnter += () => OnEnterPlay();

    // On Update
    stateMachine.State(GameState.Startup)                 .OnUpdate += () => stateMachine.GotoNextState(); // Allow one update so headset position is updated
    stateMachine.State(GameState.Countdown)               .OnUpdate += () => OnUpdateCountdown();
    stateMachine.State(GameState.Play)                    .OnUpdate += () => OnUpdatePlay();
    
    // On Exit
    stateMachine.State(GameState.Ready)                   .OnExit += () => OnExitReady();
    stateMachine.State(GameState.Play)                    .OnExit += () => OnExitPlay();
  }

  void Awake()
  {
    // Set target frame rate to 60 Hz. This will be overwritten by SimulatedUser during simulations
    Application.targetFrameRate = 60;
    
    // Get ray line renderer and hammer renderer
    _lineVisual = rightController.GetComponent<XRInteractorLineVisual>();
    _hammer = rightController.GetNamedChild("Hammer");

    // Ray line is enabled by default, and hammer is disabled
    _lineVisual.enabled = true;
    _hammer.SetActive(false);
    
    // Initialise logger
    logger = new UserInTheBox.Logger();
    
    // Initialise play parameters
    playParameters = new PlayParameters();

    // Don't show countdown text
    countdownText.enabled = false;

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
  
  public void RecordPunch(int targetID, string targetPositionStr) {
    Points = _points + PunchValue;
    _punches += 1;
    pointCounterText.text = _points.ToString();
    targetArea.RemoveTarget(targetID);
    if (logger.Active)
    {
      logger.PushWithTimestamp("events", "target_hit, " + targetID + ", " + targetPositionStr);
    }
  }

  public void RecordMiss(int targetID, string targetPositionStr)
  {
    _misses += 1;
    targetArea.RemoveTarget(targetID);
    if (logger.Active)
    {
      logger.PushWithTimestamp("events", "target_miss, " + targetID + ", " + targetPositionStr);
    }
  }
  
  public void RecordBombDetonation(int bombID, string bombPositionStr) {
    if (logger.Active)
    {
      logger.PushWithTimestamp("events", "bomb_detonate, " + bombID + ", " + bombPositionStr);
    }
    _terminate = true;
    targetArea.RemoveBomb(bombID);
  }

  public void RecordBombDisarm(int bombID, string bombPositionStr)
  {
    if (logger.Active)
    {
      logger.PushWithTimestamp("events", "bomb_disarm, " + bombID + ", " + bombPositionStr);
    }
    targetArea.RemoveBomb(bombID);
  }

  
  void InitRun() {
    // I don't think these are used for anything
    var uid = System.Guid.NewGuid().ToString();
    currentRunId = new RunIdentification();
    currentRunId.uuid = uid;
    currentRunId.startWallTime = System.DateTime.Now.ToString(Globals.Instance.timeFormat);
    currentRunId.startRealTime = Time.realtimeSinceStartup;
    
    // Set logger to target area so we can log when new targets are spawned
    targetArea.SetLogger(logger);
    
    // Hide target area
    targetArea.gameObject.SetActive(false);
    
    // Show level chooser
    levelChooser.SetActive(true);
  }
  
  void OnEnterPlay() 
  {
    // Increment round, start points from zero
    Points = 0;
    _punches = 0;
    _misses = 0;
    _roundStart = Time.time;
    _terminate = false;
    
    // Reset target id counter
    targetArea.Reset();
    
  }
  
  void OnUpdatePlay() 
  {
    // Update timer
    float elapsed = Time.time - _roundStart;
    roundCounterText.text = (elapsed >= RoundLength ? 0 : RoundLength - elapsed).ToString("N1");

    // Update point counter text
    // pointCounterText.text = (100*_punches / (_punches + _misses)).ToString("N0") + " %";

    if (logger.Active)
    {
      // Log position
      logger.PushWithTimestamp("states", UitBUtils.GetStateString(Globals.Instance.simulatedUser));
    }

    // Check if time is up for this trial; or if we should terminate for other reasons
    if ((Time.time - _roundStart > RoundLength) || _terminate)
    {
        stateMachine.GotoState(GameState.Ready);
    }
    else
    {
      // Continue play; potentially spawn a new target and/or a new bomb
      targetArea.SpawnTarget();
      targetArea.spawnBomb();
    }
  }

  void OnExitPlay()
  {
    // Stop playing
    if (PlayStop != null) PlayStop();

    // Hide target area
    targetArea.gameObject.SetActive(false);

    // Show level chooser
    levelChooser.SetActive(true);

    if (logger.Active)
    {
      // Stop logging
      logger.Finalise("states");
      logger.Finalise("events");
    }
  }

  void OnEnterReady() 
  {    
    // Hide hammer, show ray
    _lineVisual.enabled = true;
    _hammer.SetActive(false);
  }
  
  void OnExitReady() 
  {
    // Display target area
    targetArea.gameObject.SetActive(true);
    
    // Play parameters have been chosen, update them
    targetArea.SetPlayParameters(playParameters);
    
    // Enable logger if not training
    logger.Active = !playParameters.isCurrentTraining;
    
    // Set target area to correct position
    targetArea.SetPosition(headset);
    
    // Scale target area correctly
    targetArea.SetScale();
    
    // Set also position of point screen (showing timer/score)
    Vector3 pointScreenOffset = new Vector3(-0.5f, 0.0f, 2.5f);
    pointScreen.SetPositionAndRotation(headset.position + pointScreenOffset, Quaternion.identity);
    
    // Hide the controller ray, show hammer
    _lineVisual.enabled = false;
    _hammer.SetActive(true);
    
    // Hide level chooser
    levelChooser.SetActive(false);
    
    // Initialise log files
    if (logger.Active)
    {
      logger.Initialise("states");
      logger.Initialise("events");
      
      // Write level and random seed
      logger.Push("states", "level " + playParameters.currentLevel + 
                            ", random seed " + playParameters.randomSeed);
      
      // Write headers
      logger.Push("states", UitBUtils.GetStateHeader());
      logger.Push("events", "timestamp, type, target_id, target_pos_x, target_pos_y, target_pos_z");
      
      // Do first logging here, so we get correctly positioned controllers/headset when game begins
      logger.PushWithTimestamp("states", UitBUtils.GetStateString(Globals.Instance.simulatedUser));
    }

  }
  
  void OnEnterCountdown()
  {
    _countdownStart = Time.time;
    countdownText.text = (CountdownDuration-1).ToString();
    countdownText.enabled = true;
  }
  void OnUpdateCountdown()
  {
    float elapsed = CountdownDuration - (Time.time - _countdownStart);
    countdownText.text = Math.Max(elapsed, 1).ToString("N0");

    // Go to Play state after countdown reaches zero
    if (elapsed <= 0)
    {
      countdownText.enabled = false;
      stateMachine.GotoNextState();
    }
    // Hide after one
    else if (elapsed <= 1 && countdownText.text != "Go!")
    {
      countdownText.text = "Go!";
    }
  }

  public void SetLevel(string level, int randomSeed)
  {
    playParameters.SetLevel(level, false, randomSeed);
    
    // Visit Ready state, as some important stuff will be set (on exit)
    stateMachine.GotoState(GameState.Ready);
    
    // Make sure target area is correctly set (would be set after exiting GameState.Ready, but controllers/headset 
    // pos/rot might not be set yet

    // Start playing
    stateMachine.GotoState(GameState.Play);

  }
}
