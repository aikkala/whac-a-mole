using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TargetArea : MonoBehaviour
{
    public Target target;
    private PlayParameters _playParameters;

    // Target area position is always the same
    public Vector3 TargetAreaPosition => new Vector3(0.1f, -0.1f, 0.40f); 
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
        transform.SetPositionAndRotation(headset.position + TargetAreaPosition, TargetAreaRotation);
    }

    public bool SpawnTarget()
    {
        // Sample a new target with given probability
        if (Random.Range(0f, 1f) > _playParameters.SpawnProbability || transform.childCount >= _playParameters.MaxTargets+1)
        {
            return false;
        } 
        
        // Instantiate a new target
        Target newTarget = Instantiate(target, transform.position, transform.rotation, transform);

        // Sample target location, size, life span
        newTarget.Position = SamplePosition();
        newTarget.Size = SampleSize();
        newTarget.LifeSpan = SampleLifeSpan();
        return true;
    }

    private Vector3 SamplePosition()
    {
        // Go through all existing targets, sample position until a suitable one is found (not overlapping other
        // targets). If a suitable position isn't found in 20 attempts, just use whatever position is latest
        float h = 0, w = 0, d = 0;
        for (var idx = 0; idx < 20; idx++)
        {
            h = Random.Range(-_playParameters.TargetAreaHeight/2, _playParameters.TargetAreaHeight/2);
            w = Random.Range(-_playParameters.TargetAreaWidth/2, _playParameters.TargetAreaWidth/2);
            d = Random.Range(-_playParameters.TargetAreaDepth/2, _playParameters.TargetAreaDepth/2);
            var pos = new Vector3(h, w, d);
            var good = true;
            
            foreach (var t in gameObject.GetComponentsInChildren<Target>())
            {
                // If the suggested position overlaps with the position of another target, break and sample a new
                // position
                if (Vector3.Distance(t.Position, pos) < t.Size)
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
        return new Vector3(w, h, d);
    }

    private float SampleSize()
    {
        return Random.Range(_playParameters.TargetSize[0], _playParameters.TargetSize[1]);
    }

    private float SampleLifeSpan()
    {
        return Random.Range(_playParameters.TargetLifeSpan[0], _playParameters.TargetLifeSpan[1]);
    }
}
