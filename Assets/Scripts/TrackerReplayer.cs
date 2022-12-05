using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackerReplayer : MonoBehaviour {
  public Transform headTransform;
  public Transform leftHandTransform;
  public Transform rightHandTransform;
  
  public TextAsset trackerDataAsset;

  private TrackerDataFormat trackerData;
  
  private float currentStateStartTime;
  
  private float stateStartTime;
  private int currentFrame = 0;
  
  private bool loaded = false;

  void Start() {
    if (trackerDataAsset != null) {
      trackerData = JsonUtility.FromJson<TrackerDataFormat>(trackerDataAsset.text);

      Debug.Log("Replay run id: " + trackerData.runIdentification.uuid);
      Debug.Log("Replay start walltime: " + trackerData.runIdentification.startWallTime);
      Debug.Log("Replay start realtime: " + trackerData.runIdentification.startRealTime);
      Debug.Log("Replay tracker data count: " + trackerData.trackerData.Count); 
      Debug.Log("Replay tracker data perm: " + trackerData.runIdentification.permutation); 

      Globals.Instance.sequenceManager.stateMachine.AnyStateEnter += StateEnter;
      
      // Globals.Instance.sequenceManager.SetPermutation(trackerData.runIdentification.permutation);
      
      var idx = 1;
      var cState = "";
      
      while (idx > 0 && idx < trackerData.trackerData.Count) {
        idx = trackerData.trackerData.FindIndex(idx, x => x.state != cState);
        if (idx > 0) {
          cState = trackerData.trackerData[idx].state;
          Debug.Log("State: " + cState + " start at frame " + idx + " (" + trackerData.trackerData[idx].time + ")");
        }
      }

      UpdateTransforms(
        trackerData.trackerData[0].hmd,
        trackerData.trackerData[0].lhand,
        trackerData.trackerData[0].rhand
      );
      
      loaded = true;
    }
  }
  
  void UpdateTransforms(TrackerTransform hmd, TrackerTransform lhand, TrackerTransform rhand) {
    headTransform.localPosition = hmd.position;
    headTransform.localRotation = hmd.rotation;

    leftHandTransform.localPosition = lhand.position;
    leftHandTransform.localRotation = lhand.rotation;

    rightHandTransform.localPosition = rhand.position;
    rightHandTransform.localRotation = rhand.rotation;
  }

  void StateEnter(GameState state) {
    stateStartTime = Time.realtimeSinceStartup;
    currentFrame = 0;

    var stateName = state.ToString("g");
    
    var idx = trackerData.trackerData.FindIndex(x => x.state == stateName);
    
    if (idx >= 0) {
      currentStateStartTime = trackerData.trackerData[idx].time;
    }
  }

  void Update() {
    if (loaded && trackerData.trackerData.Count > currentFrame) {
      var relativeSeqTime = Time.realtimeSinceStartup - stateStartTime;
      var recTargetTime = currentStateStartTime + relativeSeqTime;

      var firstFutureIndex = trackerData.trackerData.FindIndex(currentFrame, x => x.time > recTargetTime);
      
      if (firstFutureIndex > 0) {
        currentFrame = firstFutureIndex - 1;

        var hmd = trackerData.trackerData[currentFrame].hmd;
        var lhand = trackerData.trackerData[currentFrame].lhand;
        var rhand = trackerData.trackerData[currentFrame].rhand;

        UpdateTransforms(hmd, lhand, rhand);
      }
    }
  }
}
