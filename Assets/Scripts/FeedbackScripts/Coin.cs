using UnityEngine;

public class Coin : ArcMotion
{
    [SerializeField] float speed = 5f;
    [SerializeField] float arcHeight = 2f;

    private float timeInBox = 0.5f;
    private float currentTime = 0;

    private TrailRenderer tr;

    private bool hasArrived;

    public void SetTarget(Transform targetTRF)
    {
        BeginArcMotion(targetTRF.position, speed, arcHeight);

        if(tr == null)
        {
            tr = GetComponent<TrailRenderer>();
        }

        currentTime = 0;
        hasArrived = false;
        ActiveTrail();
    }

    protected override void Update()
    {
        base.Update();
        if (!hasArrived || !gameObject.activeSelf) return;

        if (currentTime < timeInBox)
        {
            currentTime += Time.deltaTime;
        }
        else
        {
            tr.emitting = false;
            ObjectPoolManager.ReturnObjectToPool(gameObject);
        }
    }

    protected override void OnArcMotionCompleted()
    {
        hasArrived = true;
        EventBus.Publish(new OnArcMotionEnded(this));
    }

    public void ActiveTrail()
    {
        tr.Clear();
        tr.emitting = true;
    }
}