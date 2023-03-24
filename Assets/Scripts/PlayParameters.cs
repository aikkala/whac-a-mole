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
  public bool isCurrentTraining;
  public float roundLength;
  public Func<Vector3, bool> velocityThreshold;
  public string game = "difficulty";
  public string level = "level1";

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
  
  public void Initialise(bool isTraining, int fixedSeed=0) {
    
    targetLifeSpan = new Vector2(2.0f, 2.0f);
    targetSize = new Vector2(0.025f, 0.025f);
    targetSpawnBan = new Vector2(0.0f, 0.5f);
    bombSpawnBan = new Vector2(0.0f, 0.5f);
    isCurrentTraining = isTraining;
    roundLength = isTraining ? 60 : 60;
    
    if (game + "-" + level == "difficulty-level1")
    {
      maxTargets = 1;
      maxBombs = 0;
      targetAreaPosition = new Vector3(0.1f, -0.1f, 0.4f);
      targetAreaRotation = Quaternion.identity;
      velocityThreshold = velocity => velocity.z > 0.6f;
    }
    else if (game + "-" + level == "difficulty-level2")
    {
      maxTargets = 3;
      maxBombs = 0;
      targetAreaPosition = new Vector3(0.1f, -0.1f, 0.4f);
      targetAreaRotation = Quaternion.identity;
      velocityThreshold = velocity => velocity.z > 0.6f;
    }
    else if (game + "-" + level == "difficulty-level3")
    {
      maxTargets = 5;
      maxBombs = 0;
      targetAreaPosition = new Vector3(0.1f, -0.1f, 0.4f);
      targetAreaRotation = Quaternion.identity;
      velocityThreshold = velocity => velocity.z > 0.6f;
    }
    else if (game + "-" + level == "effort-level1")
    {
      maxTargets = 3;
      maxBombs = 0;
      targetAreaPosition = new Vector3(0.1f, -0.3f, 0.4f);
      targetAreaRotation = new Quaternion(0.3826834f, 0, 0, 0.9238795f);
      velocityThreshold = velocity => velocity.y < -0.6f;
    }
    else if (game + "-" + level == "effort-level2")
    {
      maxTargets = 3;
      maxBombs = 0;
      targetAreaPosition = new Vector3(0.1f, -0.1f, 0.4f);
      targetAreaRotation = Quaternion.identity;
      velocityThreshold = velocity => velocity.z > 0.6f;
    }
    else if (game + "-" + level == "effort-level3")
    {
      maxTargets = 3;
      maxBombs = 0;
      targetAreaPosition = new Vector3(0.1f, 0.2f, 0.3f);
      targetAreaRotation = new Quaternion(-0.3826834f, 0, 0, 0.9238795f);
      velocityThreshold = velocity => velocity.z > 0.6f;
    }
    else if (game + "-" + level == "unconstrained-level1")
    {
      maxTargets = 1;
      maxBombs = 0;
      targetAreaPosition = new Vector3(0.1f, -0.1f, 0.4f);
      targetAreaRotation = Quaternion.identity;
      velocityThreshold = velocity => true;
    }
    else if (game + "-" + level == "unconstrained-level2")
    {
      maxTargets = 3;
      maxBombs = 0;
      targetAreaPosition = new Vector3(0.1f, -0.1f, 0.4f);
      targetAreaRotation = Quaternion.identity;
      velocityThreshold = velocity => true;
    }
    else if (game + "-" + level == "unconstrained-level3")
    {
      maxTargets = 5;
      maxBombs = 0;
      targetAreaPosition = new Vector3(0.1f, -0.1f, 0.4f);
      targetAreaRotation = Quaternion.identity;
      velocityThreshold = velocity => true;
    }
    else
    {
      throw new NotImplementedException("Play parameters not defined for game " + game + " and level " + level);
    }
    
    // Sample and set a random seed
    randomSeed = fixedSeed == 0 ? Random.Range(0, 1000000) : fixedSeed;
    Random.InitState(randomSeed);
  }
}