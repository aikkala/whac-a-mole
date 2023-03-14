using UnityEngine;
using UnityEngine.UI;

public class StartPlay : MonoBehaviour
{
    void Start()
    {
        Button btn = gameObject.GetComponent<Button>();
        btn.onClick.AddListener(StartGame);
    }

    void StartGame()
    {
        // Start playing
        Globals.Instance.sequenceManager.stateMachine.GotoState(GameState.Countdown);
    }
}