using UnityEngine;
using UnityEngine.UI;

public class StartTraining : MonoBehaviour
{
    public UserInTheBox.Logger logger;
    public bool constrained = true;
    
    void Start()
    {
        Button btn = gameObject.GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        // Always train with difficulty=medium (constrained or unconstrained version)
        string condition = constrained ? "medium" : "medium-unconstrained";
        Globals.Instance.sequenceManager.playParameters.condition = condition;

        // Disable logging
        logger.enabled = false;

        // Start playing
        Globals.Instance.sequenceManager.stateMachine.GotoState(GameState.Countdown);
    }
}