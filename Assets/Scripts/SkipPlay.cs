using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class SkipPlay : MonoBehaviour
{
    void Start()
    {
        Button btn = gameObject.GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        Globals.Instance.sequenceManager.stateMachine.GotoState(GameState.Ready);
    }
}
