using Unity.VisualScripting;
using UnityEngine;

//estatico y ya fue?

public static class TrainRanges
{

    static public float positiveLimit;
    static public float negativeLimit;

    public static void SetRanges(float range, Vector3 headPosition)
    {
        positiveLimit = headPosition.z + (float)range;
        negativeLimit = headPosition.z - (float)range;
    }

}
