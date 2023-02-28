using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayParameters {

  // Play parameters
  public int maxTargets;
  public Vector2 targetLifeSpan;
  public Vector2 targetSize;
  public Vector2 targetSpawnBan;
  public int randomSeed;

  // Target area parameters
  // public Vector3 TargetAreaPosition => new Vector3(0.1f, -0.2f, 0.4f); 
  // public Quaternion TargetAreaRotation => new Quaternion(0.3826834f, 0, 0, 0.9238795f);
  // public Vector3 TargetAreaPosition => new Vector3(0.1f, -0.1f, 0.4f); 
  // public Quaternion TargetAreaRotation => new Quaternion(0, 0, 0, 1);
  public Vector3 targetAreaPosition;
  public Quaternion targetAreaRotation;
  public float targetAreaHeight, targetAreaWidth, targetAreaDepth;

  public PlayParameters()
  {
    // The position and size of target area is the same in all levels of difficulty
    targetAreaHeight = 0.3f;
    targetAreaWidth = 0.3f;
    targetAreaDepth = 0.001f;

    targetAreaPosition = new Vector3(0.1f, -0.1f, 0.4f);
    targetAreaRotation = Quaternion.identity;
  }
  
  public void SetLevel(string level, bool isTraining) {
    
    targetLifeSpan = new Vector2(3.0f, 3.0f);
    targetSize = new Vector2(0.025f, 0.025f);
    targetSpawnBan = new Vector2(0.0f, 0.5f);

    if (level == "easy")
    {
      maxTargets = 1;
      randomSeed = 111;
    }
    else if (level == "medium")
    {
      maxTargets = 3;
      randomSeed = 333;
    }
    else if (level == "hard")
    {
      maxTargets = 5;
      randomSeed = 555;
    }
    else if (level == "random")
    {
      // Choose max number of targets randomly from a set of 1, 3, 5
      List<int> numTargets = new List<int> { 1, 3, 5 };
      int randomIndex = Random.Range(0, numTargets.Count);
      maxTargets = numTargets[randomIndex];
    }
    else
    {
      throw new NotImplementedException("Play parameters not defined for level " + level);
    }
    
    // If not in training mode, set random seed
    if (!isTraining)
    {
      Random.InitState(randomSeed);
    }
  }
}