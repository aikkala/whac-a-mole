using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LineCircleCreator))]
public class LineCircleCreatorInspector : Editor {

  public override void OnInspectorGUI() {
    LineCircleCreator target = (LineCircleCreator) this.target;

    DrawDefaultInspector();
    
    if (GUILayout.Button("Generate")) {
      target.GenerateCircle();
    }
  }
}


[CustomEditor(typeof(ScaleToggle))]
public class ScaleToggleInspector : Editor {

  public override void OnInspectorGUI() {
    ScaleToggle target = (ScaleToggle) this.target;

    DrawDefaultInspector();
    
    if (GUILayout.Button("Scale Up")) {
      target.Show(true);
    }
    if (GUILayout.Button("Scale Down")) {
      target.Show(false);
    }
  }
}