using UnityEngine;

public class Coal : ArcMotion
{
    private float coalAmount;
    public float CoalAmount => coalAmount;
    public void SetTarget(Transform targetTRF)
    {
        BeginArcMotion(targetTRF.position, speed, arcHeight);
    }
    public void SetCoalAmount(float amount)
    {
        coalAmount = amount;
    }
}