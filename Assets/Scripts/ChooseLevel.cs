using UnityEngine;
using UnityEngine.UI;

public class ChooseLevel : MonoBehaviour
{
    public string level;
    
    void Start()
    {
        Button btn = gameObject.GetComponent<Button>();
        btn.onClick.AddListener(SetLevel);
    }

    void SetLevel()
    {
        Globals.Instance.sequenceManager.playParameters.level = level;
    }
}
