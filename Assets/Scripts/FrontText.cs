using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FrontText : MonoBehaviour {
  public TextMeshPro text;

  public AnimationCurve scaleCurve;

  public float scaleTimeFactor;

  private float spawnStartTime;
  private float shrinkStartTime;
  
  private bool shrinking = false;
  private float scaleOnShrinkStart = 1.0f;

  void Start() {
    spawnStartTime = Time.time;
    transform.localScale = new Vector3(0, 0, 0);

    Globals.Instance.sequenceManager.FrontTextShrink += Shrink;
  }

  void Update() {
    if (!shrinking) {
      var ctime = ((Time.time - spawnStartTime) * scaleTimeFactor) - 1;
      ctime = Mathf.Clamp(ctime, -1, 0);

      var eval = scaleCurve.Evaluate(ctime);
      
      transform.localScale = new Vector3(eval, eval, eval);
    } else {
      var ctime = (Time.time - shrinkStartTime) * scaleTimeFactor;
      ctime = Mathf.Clamp(ctime, 0, 1);

      var eval = scaleCurve.Evaluate(ctime);
      
      eval = eval * scaleOnShrinkStart;

      transform.localScale = new Vector3(eval, eval, eval);
      
      if (ctime >= 1) {
        DestroyThis();
      }
    }
  }

  public void Shrink() {
    scaleOnShrinkStart = transform.localScale.x;

    shrinkStartTime = Time.time;
    shrinking = true;
  }
  
  public void DestroyThis() {
    Globals.Instance.sequenceManager.FrontTextShrink -= Shrink;
    Destroy(this.gameObject);
  }

}
