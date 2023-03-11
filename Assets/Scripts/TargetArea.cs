using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TargetArea : MonoBehaviour
{
    public Target target;
    public Bomb bomb;
    private float _spawnBan;
    private float _bombSpawnBan;
    private PlayParameters _playParameters;
    private int _objectID;
    private UserInTheBox.Logger _logger;
    private int _numTargets;
    private int _numBombs;
    private Dictionary<int, Tuple<Vector3, float>> objects;

    public void Awake()
    {
        objects = new Dictionary<int, Tuple<Vector3, float>>();
    }

    public void Reset()
    {
        _numTargets = 0;
        _numBombs = 0;
        _objectID = 0;
        objects.Clear();
    }

    public void RemoveBomb(int id)
    {
        objects.Remove(id);
        _numBombs -= 1;
    }

    public void RemoveTarget(int id)
    {
        objects.Remove(id);
        _numTargets -= 1;
    }
    
    public void SetLogger(UserInTheBox.Logger logger)
    {
        _logger = logger;
    }
    public void SetPlayParameters(PlayParameters playParameters)
    {
        _playParameters = playParameters;
    }
    
    public void SetPosition(Transform headset)
    {
        transform.SetPositionAndRotation(headset.position + _playParameters.targetAreaPosition, 
            _playParameters.targetAreaRotation);
    }

    public void SetScale()
    {
        transform.Find("area").transform.localScale = new Vector3(
            _playParameters.targetAreaWidth, 
            _playParameters.targetAreaHeight,
            _playParameters.targetAreaDepth
        );
    }
    
    public bool SpawnTarget()
    {
        // Sample a new target after spawn ban has passed OR if there are no targets
        // if (Random.Range(0f, 1f) > _playParameters.SpawnProbability || transform.childCount >= _playParameters.MaxTargets+1)
        if (_numTargets >= _playParameters.maxTargets)
        {
            return false;
        } 
        
        if (Time.time > _spawnBan || _numTargets == 0 )
        {
            // Instantiate a new target
            Target newTarget = Instantiate(target, transform.position, transform.rotation, transform);

            // Sample new spawn ban time
            _spawnBan = Time.time + SampleSpawnBan();

            // Sample target location, size, life span
            newTarget.Size = SampleSize();
            newTarget.Position = SamplePosition(newTarget.Size);
            newTarget.LifeSpan = SampleLifeSpan();

            // Increase number of targets
            _numTargets += 1;
            
            // Set ID and increment counter
            newTarget.ID = _objectID;
            _objectID += 1;
            
            // Add to objects
            objects.Add(newTarget.ID, new Tuple<Vector3, float>(newTarget.Position, newTarget.Size));

            if (_logger.Active)
            {
                // Log the event
                _logger.PushWithTimestamp("events", "spawn_target, " + newTarget.ID + ", "
                                                    + newTarget.PositionToString());
            }

            return true;
        }

        return false;
    }

    public bool spawnBomb()
    {
        // Sample a new bomb after spawn ban has passed
        if (_numBombs >= _playParameters.maxBombs)
        {
            return false;
        } 
        
        if (Time.time > _bombSpawnBan)
        {
            // Instantiate a new target
            Bomb newBomb = Instantiate(bomb, transform.position, transform.rotation, transform);

            // Sample new spawn ban time
            _bombSpawnBan = Time.time + SampleBombSpawnBan();

            // Sample target location, size, life span
            newBomb.Size = SampleSize();
            newBomb.Position = SamplePosition(newBomb.Size);
            newBomb.LifeSpan = SampleLifeSpan();

            _numBombs += 1;
            
            // Set ID and increment counter
            newBomb.ID = _objectID;
            _objectID += 1;
            
            // Add to objects
            objects.Add(newBomb.ID, new Tuple<Vector3, float>(newBomb.Position, newBomb.Size));

            if (_logger.Active)
            {
                // Log the event
                _logger.PushWithTimestamp("events", "spawn_bomb, " + newBomb.ID + ", " 
                                                    + newBomb.PositionToString());
            }

            return true;
        }
        
        return false;
    }

    private Vector3 SamplePosition(float targetSize)
    {
        // Go through all existing targets, sample position until a suitable one is found (not overlapping other
        // targets). If a suitable position isn't found in 20 attempts, just use whatever position is latest
        float x = 0, y = 0, z = 0;
        Vector3 pos = new Vector3(x, y, z);
        int idx = 0;
        for (; idx < 10; idx++)
        {
            x = Random.Range(-_playParameters.targetAreaWidth/2, _playParameters.targetAreaWidth/2);
            y = Random.Range(-_playParameters.targetAreaHeight/2, _playParameters.targetAreaHeight/2);
            z = Random.Range(-_playParameters.targetAreaDepth/2, _playParameters.targetAreaDepth/2) + targetSize;
            pos = new Vector3(x, y, z);
            var good = true;
            
            foreach (var objectInfo in objects.Values)
            {
                // If the suggested position overlaps with the position of another object, break and sample a new
                // position.
                if (Vector3.Distance(objectInfo.Item1, pos) < (objectInfo.Item2+targetSize))
                {
                    good = false;
                    break;
                }
            }

            // If the found position was good, break the loop
            if (good)
            {
                break;
            }
            
        }
        
        return pos;
    }

    private float SampleSize()
    {
        return Random.Range(_playParameters.targetSize[0], _playParameters.targetSize[1]);
    }

    private float SampleLifeSpan()
    {
        return Random.Range(_playParameters.targetLifeSpan[0], _playParameters.targetLifeSpan[1]);
    }

    private float SampleSpawnBan()
    {
        return Random.Range(_playParameters.targetSpawnBan[0], _playParameters.targetSpawnBan[1]);
    }

    private float SampleBombSpawnBan()
    {
        return Random.Range(_playParameters.bombSpawnBan[0], _playParameters.bombSpawnBan[1]);
    }
}
