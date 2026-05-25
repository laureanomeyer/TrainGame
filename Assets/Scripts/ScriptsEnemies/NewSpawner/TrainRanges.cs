using Unity.VisualScripting;
using UnityEngine;

public class TrainRanges
{
    public (float, float) SetRanges(float range, Vector3 headPosition)
    {
        float positiveLimit = headPosition.z + range;
        float negativeLimit = headPosition.z - range;

        return (positiveLimit, negativeLimit);
    }

}
