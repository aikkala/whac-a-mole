using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingIcon : MonoBehaviour {
  public Transform[] circles;
  
  public float speedFactor;
  public float diffFactor;

  public float maxScale;
  public float minScale;

  void Start() {
      
  }

  void Update() {
    var time = Time.timeSinceLevelLoad;

    int i = 0;
    foreach(var circle in circles) {
      var scale = Mathf.Sin((time + i * diffFactor) * speedFactor );
      scale = Mathf.Abs(scale);

      scale = Mathf.Lerp(minScale, maxScale, scale);

      circle.localScale = new Vector3(scale, 1.0f, scale);
      i++;
    }
  }
}
