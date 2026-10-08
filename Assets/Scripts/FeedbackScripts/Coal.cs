using UnityEngine;

public class Coal : ArcMotion
{

    public void SetTarget(Transform targetTRF)
    {
        BeginArcMotion(targetTRF.position, speed, arcHeight);
    }
}