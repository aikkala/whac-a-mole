using UnityEngine;
using UnityEngine.UI;

public class InitLevel : MonoBehaviour
{
    public string level;
    public bool isTraining;
    
    void Start()
    {
        Button btn = gameObject.GetComponent<Button>();
        btn.onClick.AddListener(SetLevel);
    }

    void SetLevel()
    {
        // Set play parameters
        Globals.Instance.sequenceManager.playParameters.SetLevel(level, isTraining);
    }
}
