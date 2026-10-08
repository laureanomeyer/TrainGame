using UnityEngine;

public class Coin : ArcMotion
{
    public void SetTarget(Transform targetTRF)
    {
        BeginArcMotion(targetTRF.position, speed, arcHeight);
    }
}