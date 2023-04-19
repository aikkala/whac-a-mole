using UnityEngine;
using UnityEngine.UI;

public class StartExperiment : MonoBehaviour
{
    public UserInTheBox.Logger logger;
    
    void Start()
    {
        Button btn = gameObject.GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        // Initialise experiment
        Globals.Instance.sequenceManager.InitExperiment();
        
        // Enable logging
        logger.enabled = true;

        // Start playing
        Globals.Instance.sequenceManager.stateMachine.GotoState(GameState.Countdown);
    }
}