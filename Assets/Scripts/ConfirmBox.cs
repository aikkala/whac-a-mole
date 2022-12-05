using UnityEngine;
using TMPro;

public class ConfirmBox: MonoBehaviour {
  
  public TextMeshPro noteText;
  public ScaleToggle scaleToggle;
  public bool isActive = false;
  public event System.Action triggered;
  
  private void OnTriggerEnter(Collider other) {
     if (isActive) {
       if (triggered != null) triggered();
     }
  }

  public void Show(bool show, string text) {
    isActive = show;
    scaleToggle.Show(show);
    noteText.text = text;
  }
}
