using UnityEngine;

public class Coal : ArcMotion
{
    [SerializeField] float speed = 50f;
    [SerializeField] float arcHeight = 15f;

    public void SetTarget(Transform targetTRF)
    {
        BeginArcMotion(targetTRF.position, speed, arcHeight);
    }

    protected override void OnArcMotionCompleted()
    {
        ObjectPoolManager.ReturnObjectToPool(gameObject);
    }
}