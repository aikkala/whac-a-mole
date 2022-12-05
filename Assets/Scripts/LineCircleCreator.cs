using UnityEngine;
using System.Collections;
using UnityEditor;


public class LineCircleCreator : MonoBehaviour  {
  public float radius;
  public int circleSegments;
  public int drawSegments;
  public bool loop; 
    
  public void GenerateCircle() {
    var lineRenderer = GetComponent<LineRenderer>();

    var points = new Vector3[drawSegments];
        
    for (int i = 0; i < drawSegments; i++) {
        var rad = Mathf.Deg2Rad * (i * 360f / circleSegments);
        points[i] = new Vector3(Mathf.Sin(rad) * radius, 0, Mathf.Cos(rad) * radius);
    }

    lineRenderer.loop = loop;
    
    lineRenderer.positionCount = points.Length;

    lineRenderer.SetPositions(points);
  }
}