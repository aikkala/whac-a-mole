using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleToggle : MonoBehaviour {
  public AnimationCurve scaleCurve;

  public float scaleTimeFactor;

  private float transitioStartTime;
  private float scaleOnTransitionStart = 1.0f;
  
  private bool transition = false;
  private float transitionFactor = 0;
  
  public float startScale;

  void Start() {
    transform.localScale = new Vector3(startScale, startScale, startScale);
  }

  void Update() {    
    if (transition) {
      var ttime = ((Time.time - transitioStartTime) * scaleTimeFactor);

      var ctime = ttime + transitionFactor;

      ctime = Mathf.Clamp(ctime, transitionFactor, transitionFactor + 1);
  
      var eval = scaleCurve.Evaluate(ctime);

      transform.localScale = new Vector3(eval, eval, eval);
      
      if (ttime >= 1) {
        transition = false;
      }
    } 
  }
  
  public void Show(bool show) {
    if (transitionFactor == (show ? -1 : 0)) {
      return;
    }

    scaleOnTransitionStart = transform.localScale.x;

    transitioStartTime = Time.time;
    transition = true;
    transitionFactor = show ? -1 : 0;
  }
}