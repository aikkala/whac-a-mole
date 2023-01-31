using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TargetArea : MonoBehaviour
{
    public Target target;
    private PlayParameters _playParameters;
    private float _spawnBan;

    // Target area position is always the same
    public Vector3 TargetAreaPosition => new Vector3(0.1f, -0.05f, 0.45f); 
    public Quaternion TargetAreaRotation => new Quaternion(0, 0, 0, 1);

    public void SetLevel(string level)
    {
        _playParameters = new PlayParameters(level);
        
        // Change the scale of the visualised area
        transform.Find("area").transform.localScale = new Vector3(
            _playParameters.TargetAreaWidth, 
            _playParameters.TargetAreaHeight,
            _playParameters.TargetAreaDepth
            );
        
    }

    public void SetPosition(Transform headset)
    {
        // transform.SetPositionAndRotation(headset.InverseTransformDirection(TargetAreaPosition), TargetAreaRotation);
        transform.SetPositionAndRotation(headset.position + TargetAreaPosition, TargetAreaRotation);
        // transform.LookAt(headset.transform);
    }

    public bool SpawnTarget()
    {
        // Sample a new target after spawn ban has passed OR if there are no targets
        // if (Random.Range(0f, 1f) > _playParameters.SpawnProbability || transform.childCount >= _playParameters.MaxTargets+1)
        if (transform.childCount >= _playParameters.MaxTargets + 1)
        {
            return false;
        } 
        else if (Time.time > _spawnBan || transform.childCount <= 1)
        {
            // Instantiate a new target
            Target newTarget = Instantiate(target, transform.position, transform.rotation, transform);

            // Sample new spawn ban time
            _spawnBan = Time.time + SampleSpawnBan();

            // Sample target location, size, life span
            newTarget.Size = SampleSize();
            newTarget.Position = SamplePosition(newTarget.Size);
            newTarget.LifeSpan = SampleLifeSpan();

            newTarget.Initialised = true;
            
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
            x = Random.Range(-_playParameters.TargetAreaWidth/2, _playParameters.TargetAreaWidth/2);
            y = Random.Range(-_playParameters.TargetAreaHeight/2, _playParameters.TargetAreaHeight/2);
            z = Random.Range(-_playParameters.TargetAreaDepth/2, _playParameters.TargetAreaDepth/2) + targetSize;
            pos = new Vector3(x, y, z);
            var good = true;
            
            foreach (var t in gameObject.GetComponentsInChildren<Target>())
            {
                // Skip the newly created target
                if (!t.Initialised)
                {
                    continue;
                }
                
                // If the suggested position overlaps with the position of another target, break and sample a new
                // position.
                if (Vector3.Distance(t.Position, pos) < (t.Size+targetSize))
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
        return Random.Range(_playParameters.TargetSize[0], _playParameters.TargetSize[1]);
    }

    private float SampleLifeSpan()
    {
        return Random.Range(_playParameters.TargetLifeSpan[0], _playParameters.TargetLifeSpan[1]);
    }

    private float SampleSpawnBan()
    {
        return Random.Range(_playParameters.TargetSpawnBan[0], _playParameters.TargetSpawnBan[1]);
    }
}
