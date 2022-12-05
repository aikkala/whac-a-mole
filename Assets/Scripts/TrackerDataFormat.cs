using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading;

[System.Serializable]
public struct TrackerTransform {
  public Vector3 position;
  public Quaternion rotation;
}

[System.Serializable]
public struct RunIdentification {
  public string uuid;
  public string startWallTime;
  public float startRealTime;
  public int permutation;
}

[System.Serializable]
public struct TrackerDataFormat {
  public string saveTime;

  public List<SensorData> trackerData;

  public RunIdentification runIdentification;
}

[System.Serializable]
public struct SensorData {
  public TrackerTransform hmd;
  public TrackerTransform lhand;
  public TrackerTransform rhand;
  public string state;

  public float time;
  public string walltime;

  public SensorData(float _time, string _walltime, Transform hmdTransform, Transform rhandTransform, Transform lhandTransform, string _state = "") {
    hmd.position = hmdTransform.localPosition;
    hmd.rotation = hmdTransform.localRotation;

    rhand.position = rhandTransform.localPosition;
    rhand.rotation = rhandTransform.localRotation;

    lhand.position = lhandTransform.localPosition;
    lhand.rotation = lhandTransform.localRotation;

    time = _time;
    walltime = _walltime;
    state = _state;
  }
}
