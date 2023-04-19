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
  public float roundLength;
  public Func<Vector3, bool> velocityThreshold;
  public string condition;
  public string order;

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
  
  public void Initialise(int fixedSeed=0) {
    
    targetLifeSpan = new Vector2(1.0f, 1.0f);
    targetSize = new Vector2(0.025f, 0.025f);
    targetSpawnBan = new Vector2(0.0f, 0.5f);
    bombSpawnBan = new Vector2(0.0f, 0.5f);
    roundLength = 60;
    
    if (condition == "easy" || condition == "easy-unconstrained")
    {
      maxTargets = 1;
      maxBombs = 0;
      targetAreaPosition = new Vector3(0.1f, -0.1f, 0.4f);
      targetAreaRotation = Quaternion.identity;
      velocityThreshold = velocity => velocity.z > 0.8f;
    }
    else if (condition == "medium" || condition == "medium-unconstrained")
    {
      maxTargets = 3;
      maxBombs = 0;
      targetAreaPosition = new Vector3(0.15f, -0.1f, 0.4f);
      targetAreaRotation = Quaternion.identity;
      velocityThreshold = velocity => velocity.z > 0.8f;
    }
    else if (condition == "hard" || condition == "hard-unconstrained")
    {
      maxTargets = 5;
      maxBombs = 0;
      targetAreaPosition = new Vector3(0.15f, -0.1f, 0.4f);
      targetAreaRotation = Quaternion.identity;
      velocityThreshold = velocity => velocity.z > 0.8f;
    }
    else if (condition == "low" || condition == "low-unconstrained")
    {
      maxTargets = 3;
      maxBombs = 0;
      targetAreaPosition = new Vector3(0.15f, -0.3f, 0.35f);
      targetAreaRotation = new Quaternion(0.3826834f, 0, 0, 0.9238795f);
      // velocityThreshold = velocity => velocity.y < -0.424f && velocity.z > 0.424f;
      velocityThreshold = velocity => velocity.y < -0.8f;
    }
    else if (condition == "mid" || condition == "mid-unconstrained")
    {
      maxTargets = 3;
      maxBombs = 0;
      targetAreaPosition = new Vector3(0.15f, -0.1f, 0.4f);
      targetAreaRotation = Quaternion.identity;
      velocityThreshold = velocity => velocity.z > 0.8f;
    }
    else if (condition == "high" || condition == "high-unconstrained")
    {
      maxTargets = 3;
      maxBombs = 0;
      targetAreaPosition = new Vector3(0.15f, 0.2f, 0.3f);
      targetAreaRotation = new Quaternion(-0.3826834f, 0, 0, 0.9238795f);
      velocityThreshold = velocity => velocity.z > 0.8f;
    }
    else
    {
      throw new NotImplementedException("Play parameters not defined for condition " + condition);
    }

    if (condition.Contains("unconstrained"))
    {
      velocityThreshold = velocity => true;
    }
    
    // Sample and set a random seed
    randomSeed = fixedSeed == 0 ? Random.Range(0, 1000000) : fixedSeed;
    Random.InitState(randomSeed);
  }
}