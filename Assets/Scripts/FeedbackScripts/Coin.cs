using UnityEngine;

public class Coin : ArcMotion
{
    private float coinAmount;
    public float CoinAmount => coinAmount;
    public void SetTarget(Transform targetTRF)
    {
        BeginArcMotion(targetTRF.position, speed, arcHeight);
    }

    public void SetCoinAmount(float amount)
    {
        coinAmount = amount;
    }
}