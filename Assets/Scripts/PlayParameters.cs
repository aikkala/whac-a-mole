using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayParameters {
  
  public float SpawnProbability;
  public int MaxTargets;
  public float TargetLookAheadTime;
  public Vector2 TargetLifeSpan;
  public Vector2 TargetSize;
  public Vector2 TargetSpawnBan;
  public float TargetAreaHeight, TargetAreaWidth, TargetAreaDepth;
  
  public PlayParameters(string level) 
  {
    // The position and size of target area is the same in all levels of difficulty
    TargetAreaHeight = 0.4f;
    TargetAreaWidth = 0.4f;
    TargetAreaDepth = 0.001f;
    
    // Set difficulty parameters
    if (level == "easy")
    {
      SpawnProbability = 1;
      // MaxTargets = 1;
      TargetLifeSpan = new Vector2(4f, 4f);
      TargetSize = new Vector2(0.05f, 0.05f);
    }
    else if (level == "medium")
    {
      SpawnProbability = 1f/Application.targetFrameRate;
      // MaxTargets = 3;
      TargetLifeSpan = new Vector2(1f, 3f);
      TargetSize = new Vector2(0.05f, 0.05f);
    }
    else if (level == "hard")
    {
      SpawnProbability = 1f/Application.targetFrameRate;
      // MaxTargets = 6;
      TargetLifeSpan = new Vector2(0.5f, 2f);
      TargetSize = new Vector2(0.05f, 0.05f);
    }
    else if (level == "random")
    {
      // What should SpawnProbability be? Do we want to spawn a new target as soon as one of the existing targets
      // has been hit?
      // SpawnProbability = 1f/Application.targetFrameRate;
      
      // Choose max number of targets randomly from a set of 1, 3, 5
      List<int> numTargets = new List<int> { 1, 3, 5 };
      int randomIndex = Random.Range(0, numTargets.Count);
      MaxTargets = numTargets[randomIndex];

      // float minTargetLifeSpan = Random.Range(0.5f, 2f);
      // TargetLifeSpan = new Vector2(minTargetLifeSpan, Random.Range(minTargetLifeSpan, 4f));
      TargetLifeSpan = new Vector2(2.0f, 2.0f);
      TargetSize = new Vector2(0.04f, 0.04f);
      TargetSpawnBan = new Vector2(0.1f, 1.0f);
    }
    else
    {
      throw new NotImplementedException("Play parameters not defined for level " + level);
    }
    
  }
  
}