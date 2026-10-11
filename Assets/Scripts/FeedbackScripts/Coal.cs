using UnityEngine;

public class Coal : ArcMotion
{
    [SerializeField] private float coalAmount = 1;
    public float CoalAmount => coalAmount;
    public void SetTarget(Transform targetTRF)
    {
        BeginArcMotion(targetTRF.position, speed, arcHeight);
    }
}