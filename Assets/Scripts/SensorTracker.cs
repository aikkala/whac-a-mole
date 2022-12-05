using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading;

public struct SensorAABB {
  public SensorAABB(Vector3 _min, Vector3 _max) {
    min = _min;
    max = _max;
  }
  public Vector3 min;
  public Vector3 max;
  
  public Vector3 center() {
    return new Vector3(
      min.x + (max.x - min.x) / 2,
      min.y + (max.y - min.y) / 2,
      min.z + (max.z - min.z) / 2
    );
  }

  public void includePoint(Vector3 point) {
    min = Vector3.Min(min, point);
    max = Vector3.Max(max, point);
  }

  public float getSize() {
    return Vector3.Distance(min, max);
  }
};

public class SensorTracker : MonoBehaviour {
  public static string runFolderName = "RunData";
  
  List<SensorData> data = new List<SensorData>();
    
  public bool saveThreadRunning = false;
  public bool saveThreadDone = false;
  Thread saveThread;

  void Start() {
    // Globals.Instance.sequenceManager.UpdateTrackers += RecordSensorData;

    Debug.Log("Persistent datapah: " + Application.persistentDataPath);
  }

  public void SaveData() {
    var folderpath = Application.persistentDataPath + "/" + runFolderName;

    saveThread = new Thread(() => ThreadedSave(Globals.Instance.sequenceManager.CurrentRunIdentification, folderpath));
    saveThreadRunning = true;
    saveThread.Start();
  }

  public void ResetData(RunIdentification seqid) {
    data = new List<SensorData>();
  }

  void RecordSensorData(string state)
  {
    // TODO fix this
    var hmdTranform = this.transform; //Globals.Instance.hmdTransform;
    var rhandTranform = this.transform; //Globals.Instance.rhandTransform;
    var lhandTranform = this.transform; //Globals.Instance.lhandTransform;

    var time = Time.realtimeSinceStartup - Globals.Instance.sequenceManager.CurrentRunIdentification.startRealTime;
    var walltime = System.DateTime.Now.ToString(Globals.Instance.timeFormat);
    
    data.Add(new SensorData(
      time,
      walltime,
      hmdTranform, 
      rhandTranform,
      lhandTranform,
      state
    ));
  }

  public int RecordedFrameCount() {
    return data.Count;
  }

  void ThreadedSave(RunIdentification seqid, string folderpath) {
    saveThreadRunning = true;
    saveThreadDone = false;

    while(saveThreadRunning && !saveThreadDone) {
      var outputData = new TrackerDataFormat();
      outputData.runIdentification = seqid;
      outputData.trackerData = data;

      outputData.saveTime = System.DateTime.Now.ToString(Globals.Instance.timeFormat);
      
      if (!Directory.Exists(folderpath)) {
        Directory.CreateDirectory(folderpath);
      }

      var filename = seqid.uuid.ToString() + ".json";

      var filepath = folderpath + "/" + filename;
    
      Debug.Log("Saving data to " + filepath);

      var output = JsonUtility.ToJson(outputData);
      
      File.WriteAllText(filepath, output);
      
      saveThreadDone = true;
    }

    saveThreadRunning = false;
  }

  public (SensorAABB, SensorAABB, SensorAABB) GetTrackerMovementBoundingBoxes(int steps) {
    if (data.Count - 1 < steps) return (new SensorAABB(), new SensorAABB(), new SensorAABB());

    var hmd = new SensorAABB(data[data.Count - 1].hmd.position,   data[data.Count - 1].hmd.position);
    var lhand = new SensorAABB(data[data.Count - 1].lhand.position, data[data.Count - 1].lhand.position);
    var rhand = new SensorAABB(data[data.Count - 1].rhand.position, data[data.Count - 1].rhand.position);
    
    int start = data.Count - 2;
    int end = data.Count - steps;
    
    for (int i = start; i > end; i--) {      
      hmd.includePoint(data[i].hmd.position);
      lhand.includePoint(data[i].lhand.position);
      rhand.includePoint(data[i].rhand.position);
    }

    return (hmd, lhand, rhand);
  }
}
