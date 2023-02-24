using UnityEngine;
using UnityEngine.UI;

public class ButtonClick : MonoBehaviour
{

    public SequenceManager sequenceManager;
    
    // Start is called before the first frame update
    void Start()
    {
        Button btn = gameObject.GetComponent<Button>();
        btn.onClick.AddListener(TaskOnClick);
    }

    // Update is called once per frame
    void TaskOnClick()
    {
        sequenceManager.stateMachine.GotoState(GameState.PlayRandom);
    }
}
