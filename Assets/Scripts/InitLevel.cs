using UnityEngine;
using UnityEngine.UI;

public class InitLevel : MonoBehaviour
{
    public string level;
    public bool isTraining;
    
    void Start()
    {
        Button btn = gameObject.GetComponent<Button>();
        btn.onClick.AddListener(StartPlay);
    }

    void StartPlay()
    {
        // Set play parameters
        Globals.Instance.sequenceManager.playParameters.SetLevel(level, isTraining);
        
        // Start playing
        Globals.Instance.sequenceManager.stateMachine.GotoState(GameState.Countdown);
    }
}
