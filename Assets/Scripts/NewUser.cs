using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewUser : MonoBehaviour
{
    public UserInTheBox.Logger logger;
    public TextMeshPro userText;
    
    void Start()
    {
        Button btn = gameObject.GetComponent<Button>();
        btn.onClick.AddListener(GenerateUser);
    }

    void GenerateUser()
    {
        // Create a new logging folder
        string subjectId = logger.GenerateSubjectFolder();
        
        // Set to display
        userText.text = "User ID:\n" + subjectId;
    }
}
