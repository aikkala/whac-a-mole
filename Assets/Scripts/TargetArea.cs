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
    // private bool[,] _grid;
    private List<Tuple<float, float>> _freePositions;
    // private int _gridHeight, _gridWidth;
    // private float[] _gridPosX;
    // private float[] _gridPosY;
    
    public static void Shuffle<T>(IList<T> ts) {
        // Shuffles the element order of the specified list.
        // From Smooth-P: https://forum.unity.com/threads/clever-way-to-shuffle-a-list-t-in-one-line-of-c-code.241052/
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i) {
            var r = Random.Range(i, count);
            var tmp = ts[i];
            ts[i] = ts[r];
            ts[r] = tmp;
        }
    }

    public void Awake()
    {
        objects = new Dictionary<int, Tuple<Vector3, float>>();
        _freePositions = new List<Tuple<float, float>>();
    }

    public void Reset()
    {
        _numTargets = 0;
        _numBombs = 0;
        _objectID = 0;
        objects.Clear();
    }

    public void RemoveBomb(Bomb bmb)
    {
        objects.Remove(bmb.ID);
        _numBombs -= 1;
        
        // Add this position to the list of available positions
        _freePositions.Add(new Tuple<float, float>(bmb.Position.x, bmb.Position.y));
    }

    public void RemoveTarget(Target tgt)
    {
        objects.Remove(tgt.ID);
        _numTargets -= 1;
        
        // Add this position to the list of available positions
        _freePositions.Add(new Tuple<float, float>(tgt.Position.x, tgt.Position.y));
    }
    
    public void SetLogger(UserInTheBox.Logger logger)
    {
        _logger = logger;
    }
    public void SetPlayParameters(PlayParameters playParameters)
    {
        _playParameters = playParameters;

        float targetRadius = _playParameters.targetSize[1];
        float targetDiameter = 2 * targetRadius;
        
        // Initialise grid
        int h = (int)Math.Floor(_playParameters.targetAreaHeight / targetDiameter);
        int w = (int)Math.Floor(_playParameters.targetAreaWidth / targetDiameter);
        // _grid = new bool[h, w];
        // _gridHeight = h;
        // _gridWidth = w;
        // _gridPosX = new float[w];
        // for (int i = 1; i <= w; i++)
        // {
        //     _gridPosX[i] = -(_playParameters.targetAreaWidth / 2) + (_playParameters.targetSize[1] / 2) * i;
        // }
        // _gridPosY = new float[h];
        // for (int i = 1; i <= h; i++)
        // {
        //     _gridPosY[i] = -(_playParameters.targetAreaHeight / 2) + (_playParameters.targetSize[1] / 2) * i;
        // }
        
        // Populate list of free positions
        _freePositions.Clear();
        for (int i = 0; i < w; i++)
        {
            float x = -(_playParameters.targetAreaWidth / 2) + targetRadius + targetDiameter*i;
            for (int j = 0; j < h; j++)
            {
                float y = -(_playParameters.targetAreaHeight / 2) + targetRadius + targetDiameter*j;
                _freePositions.Add(new Tuple<float, float>(x, y));
            }
        }
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
        
        if (Time.time > _spawnBan /*|| _numTargets == 0*/ )
        {
            // Instantiate a new target
            Target newTarget = Instantiate(target, transform.position, transform.rotation, transform);

            // Sample new spawn ban time
            _spawnBan = Time.time + SampleSpawnBan();

            // Sample target location, size, life span
            newTarget.Size = SampleSize();
            newTarget.Position = SampleGridPosition(newTarget.Size);
            newTarget.LifeSpan = SampleLifeSpan();
            newTarget.VelocityThreshold = _playParameters.velocityThreshold;

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
            // newBomb.Position = SamplePosition(newBomb.Size);
            newBomb.Position = SampleGridPosition(newBomb.Size);
            newBomb.LifeSpan = SampleLifeSpan();
            newBomb.VelocityThreshold = _playParameters.velocityThreshold;


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

    private Vector3 SampleGridPosition(float targetSize)
    {
        // Note! Works currently only on 2D grids
        
        // If there are no more free positions, throw an exception
        if (_freePositions.Count == 0)
        {
            throw new IndexOutOfRangeException("Out of free positions, too many targets!");
        }
        
        // Shuffle the list and return first element
        Shuffle(_freePositions);
        Tuple<float, float> pos = _freePositions[0];
        _freePositions.RemoveAt(0);
        
        // z needs to be targetSize so the target is correctly positioned on the grid
        return new Vector3(pos.Item1, pos.Item2, targetSize);
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
