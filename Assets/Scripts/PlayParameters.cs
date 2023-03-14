using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayParameters {

  // Play parameters
  public int maxTargets;
  public int maxBombs;
  public Vector2 targetLifeSpan;
  public Vector2 targetSize;
  public Vector2 targetSpawnBan;
  public Vector2 bombSpawnBan;
  public int randomSeed;
  public string currentLevel;
  public bool isCurrentTraining;

  // Target area parameters
  public Vector3 targetAreaPosition;
  public Quaternion targetAreaRotation;
  public float targetAreaHeight, targetAreaWidth, targetAreaDepth;

  public PlayParameters()
  {
    // The position and size of target area is the same in all levels of difficulty
    targetAreaHeight = 0.3f;
    targetAreaWidth = 0.3f;
    targetAreaDepth = 0.001f;
  }
  
  public void SetLevel(string level, bool isTraining, int fixedSeed=0) {
    
    targetLifeSpan = new Vector2(2.0f, 2.0f);
    targetSize = new Vector2(0.025f, 0.025f);
    targetSpawnBan = new Vector2(0.0f, 0.5f);
    bombSpawnBan = new Vector2(0.0f, 0.5f);
    currentLevel = level;
    isCurrentTraining = isTraining;
    
    if (level == "easy")
    {
      maxTargets = 1;
      maxBombs = 0;
      // randomSeed = fixedSeed == 0 ? 111 : fixedSeed;
      targetAreaPosition = new Vector3(0.1f, -0.1f, 0.4f);
      targetAreaRotation = Quaternion.identity;
    }
    else if (level == "medium")
    {
      maxTargets = 3;
      maxBombs = 0;
      // randomSeed = fixedSeed == 0 ? 333 : fixedSeed;
      targetAreaPosition = new Vector3(0.1f, -0.1f, 0.4f);
      targetAreaRotation = Quaternion.identity;
    }
    else if (level == "hard")
    {
      maxTargets = 5;
      maxBombs = 0;
      // randomSeed = fixedSeed == 0 ? 555 : fixedSeed;
      targetAreaPosition = new Vector3(0.1f, -0.1f, 0.4f);
      targetAreaRotation = Quaternion.identity;
    }
    else if (level == "up")
    {
      maxTargets = 3;
      maxBombs = 0;
      // randomSeed = fixedSeed == 0 ? 555 : fixedSeed;
      targetAreaPosition = new Vector3(0.1f, 0.2f, 0.3f);
      targetAreaRotation = new Quaternion(-0.3826834f, 0, 0, 0.9238795f);
    }
    else
    {
      throw new NotImplementedException("Play parameters not defined for level " + level);
    }
    
    // If in training mode, sample a random seed
    // if (isTraining)
    // {
    randomSeed = fixedSeed == 0 ? Random.Range(0, 1000000) : fixedSeed;
    // }
    Random.InitState(randomSeed);
  }
}