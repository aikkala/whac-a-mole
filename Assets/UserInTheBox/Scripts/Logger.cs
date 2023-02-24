using System.IO;
using UnityEngine;
using UserInTheBox;

public class Logger : MonoBehaviour
{
    public SimulatedUser simulatedUser;
    public SequenceManager sequenceManager;
    private StreamWriter _file;
    private string _baseLogFolder;
    private bool _debug = false;

    private void Awake()
    {
        if (_debug)
        {
            enabled = true;
            _baseLogFolder = "output/logging/";
        }
        else
        {
            enabled = UitBUtils.GetOptionalArgument("logging");
        }

        if (enabled)
        {
            if (enabled && !_debug)
            {
                _baseLogFolder = Path.Combine(UitBUtils.GetKeywordArgument("outputFolder"), "logging/");
            }
            Debug.Log("Logging is enabled");
            Debug.Log("Logs will be saved to " + _baseLogFolder);
        }
        else
        {
            gameObject.SetActive(false);
        }

    }

    void Start()
    {
        // Create the output directory
        Directory.CreateDirectory(_baseLogFolder);

        // Open the log file
        _file = new(Path.Combine(_baseLogFolder, "test_log.txt"));

    }
    
    void Update()
    {
        // Log position and orientation of controllers and headset
        Log(Time.time, sequenceManager.stateMachine.currentState.ToString(),
            UitBUtils.TransformToString(simulatedUser.leftHandController.transform) + ", " +
            UitBUtils.TransformToString(simulatedUser.rightHandController.transform) + ", " +
            UitBUtils.TransformToString(simulatedUser.mainCamera.transform));
    }

    public async void Log(float timestamp, string state, string msg)
    {
        // Do we want async here? Does this even work in Unity?
        await _file.WriteLineAsync(timestamp + ", " + state + "," + msg);
    }

    private void OnDestroy()
    {
        if (enabled) _file.Close();
    }
}
