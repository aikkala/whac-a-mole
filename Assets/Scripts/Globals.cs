using UnityEngine;
using TMPro;
using UserInTheBox;


public class Globals : Singleton<Globals> {
  public string timeFormat = "yyyy-MM-ddTHH:mm:ss";

  [Header("Game parameters")] 
  public float punchVelocityThreshold = 1.0f;
  
  [Space(10)]
  
  [Header("Game Global Objects")]
  
  public ConfirmBox confirmBox;
  public GameObject scoreboard;
  public SimulatedUser simulatedUser;
  public SequenceManager sequenceManager;
  public SensorTracker sensorTracker;
  public TextMeshPro debugText;
  
  protected Globals() { }
  
}