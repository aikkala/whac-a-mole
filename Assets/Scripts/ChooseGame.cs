using UnityEngine;
using UnityEngine.UI;

public class ChooseGame : MonoBehaviour
{
    public string game;
    
    void Start()
    {
        Button btn = gameObject.GetComponent<Button>();
        btn.onClick.AddListener(SetGame);
    }

    void SetGame()
    {
        Globals.Instance.sequenceManager.playParameters.game = game;
    }
}