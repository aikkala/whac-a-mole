using UnityEngine;
using UnityEngine.UI;

public class StartPlay : MonoBehaviour
{
    public UserInTheBox.Logger logger;
    
    void Start()
    {
        Button btn = gameObject.GetComponent<Button>();
        btn.onClick.AddListener(StartGame);
    }

    void StartGame()
    {
        // Initialise game
        Globals.Instance.sequenceManager.playParameters.Initialise(false);
        
        // Enable logging
        logger.enabled = true;

        // Start playing
        Globals.Instance.sequenceManager.stateMachine.GotoState(GameState.Countdown);
    }
}