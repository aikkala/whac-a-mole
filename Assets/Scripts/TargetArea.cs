using UnityEngine;
using Random = UnityEngine.Random;

public class TargetArea : MonoBehaviour
{
    public Target target;
    private float _spawnBan;
    private PlayParameters _playParameters;

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
        if (transform.childCount >= _playParameters.maxTargets + 2)
        {
            return false;
        } 
        
        if (Time.time > _spawnBan || transform.childCount <= 2)
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
            x = Random.Range(-_playParameters.targetAreaWidth/2, _playParameters.targetAreaWidth/2);
            y = Random.Range(-_playParameters.targetAreaHeight/2, _playParameters.targetAreaHeight/2);
            z = Random.Range(-_playParameters.targetAreaDepth/2, _playParameters.targetAreaDepth/2) + targetSize;
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
}
