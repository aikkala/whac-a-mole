using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UserInTheBox;

public class Replayer : MonoBehaviour
{
    public SequenceManager sequenceManager;
    private class LogData
    { 
        public Vector3 leftControllerPosition { get; set; }
        public Vector3 rightControllerPosition { get; set; }
        public Vector3 headsetPosition { get; set; }
        public Quaternion leftControllerRotation { get; set; }
        public Quaternion rightControllerRotation { get; set; }
        public Quaternion headsetRotation { get; set; }
    }

    public SimulatedUser simulatedUser;
    private List<LogData> _logData;
    private List<float> _timestamps;
    private StreamReader _reader;
    private float _startTime;
    private int _idx = 1;  // Starts from 1 because index 0 contains initial position
    private bool _debug = false;

    private void Awake()
    {
        // Check if replayer is enabled
        if (_debug)
        {
            enabled = true;
        }
        else
        {
            enabled = UitBUtils.GetOptionalArgument("replay");
        }

    }

    void Start()
    {
        // Set max delta time
        Time.fixedDeltaTime = 0.01f;
        Time.maximumDeltaTime = 0.01f;

        // Disable TrackedPoseDriver, otherwise XR Origin will always try to reset position of camera to (0,0,0)?
        simulatedUser.mainCamera.GetComponent<TrackedPoseDriver>().enabled = false;

        // Get log file path
        string filepath = UitBUtils.GetKeywordArgument("log_filepath");
        // string filepath = "/home/aleksi/Desktop/player_001/full.csv";
        
        // Read log file (csv) 
        var reader = new StreamReader(filepath);
        
        // First line contains level info
        string info = reader.ReadLine();
        
        // Second line contains the header
        string header = reader.ReadLine();
        
        // Check header matches what we expect
        if (header != UitBUtils.GetStateHeader())
        {
            throw new InvalidDataException("Header of log file " + filepath +
                                       " does not match the header set in UitBUtils.GetStateHeader()");
        }

        // Read rest of file
        _logData = new List<LogData>();
        _timestamps = new List<float>();
        while(!reader.EndOfStream)
        {
            // Read line
            string[] values = reader.ReadLine().Split(",");
            
            // Parse and add data to list
            _logData.Add(new LogData
            {
                leftControllerPosition = Str2Vec3(values[1], values[2], values[3]),
                leftControllerRotation = Str2Quat(values[4], values[5], values[6], values[7]),
                rightControllerPosition = Str2Vec3(values[8], values[9], values[10]),
                rightControllerRotation = Str2Quat(values[11], values[12], values[13], values[14]),
                headsetPosition = Str2Vec3(values[15],values[16], values[17]),
                headsetRotation = Str2Quat(values[18], values[19],values[20], values[21])
            });
            _timestamps.Add(Str2Float(values[0]));
        }
        
        // Set initial controller/headset positions/rotations
        UpdateAnchors(_logData[0]);
        
        // Get start time from first actual datum
        _startTime = _timestamps[1];
        
        // Initialise state
        InitialiseLevel(info);

    }

    public float Str2Float(string value)
    {
        return float.Parse(value);
    }

    public Vector3 Str2Vec3(string x, string y, string z)
    {
        return new Vector3(Str2Float(x), Str2Float(y), Str2Float(z));
    }

    public Quaternion Str2Quat(string x, string y, string z, string w)
    {
        return new Quaternion(Str2Float(x), Str2Float(y), Str2Float(z), Str2Float(w));
    }

    private void InitialiseLevel(string info)
    {
        // Do some parsing
        string[] values = info.Split(",");
        string level = values[0].Split(" ")[1];
        int randomSeed = int.Parse(values[1].Split(" ")[3]);
        
        // Initialise state
        sequenceManager.SetLevel(level, randomSeed);
    }

    // Update is called once per frame
    void Update()
    {
        // Stop application once all data has been played, and state changed to Ready
        if (_idx >= _timestamps.Count && sequenceManager.stateMachine.currentState == GameState.Ready)
        {
            //If we are running in a standalone build of the game
#if UNITY_STANDALONE
             Application.Quit();
#endif
 
            //If we are running in the editor
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
        
        // Do nothing if we ran out of log data (wait for game to close), or wait until Time.time catches up
        if (_idx >= _timestamps.Count || _timestamps[_idx] - _startTime > Time.time)
        {
            return;
        }
        
        // Find next timestamp in log data
        while (_idx < _timestamps.Count && _timestamps[_idx] - _startTime < Time.time)
        {
            _idx += 1;
        }
        
        // Update anchors
        if (_idx < _logData.Count)
        {
            UpdateAnchors(_logData[_idx]);
        }
    }

    private void UpdateAnchors(LogData data)
    {
        simulatedUser.mainCamera.transform.SetPositionAndRotation(data.headsetPosition, data.headsetRotation);
        simulatedUser.leftHandController.SetPositionAndRotation(data.leftControllerPosition, 
            data.leftControllerRotation);
        simulatedUser.rightHandController.SetPositionAndRotation(data.rightControllerPosition, 
            data.rightControllerRotation);
    }
}
