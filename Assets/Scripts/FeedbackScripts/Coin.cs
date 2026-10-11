using UnityEngine;

public class Coin : ArcMotion
{
    [SerializeField] private float coinAmount;
    public float CoinAmount => coinAmount;
    public void SetTarget(Transform targetTRF)
    {
        BeginArcMotion(targetTRF.position, speed, arcHeight);
    }
}