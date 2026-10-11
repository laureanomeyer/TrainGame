using UnityEngine;

public abstract class ArcMotion : MonoBehaviour
{
    private ArcMover arcMover;
    protected bool hasCompleted;
    [SerializeField] protected float speed = 5f;
    [SerializeField] protected float arcHeight = 2f;
    protected TrailRenderer tr;
    protected float timeInBox = 0.5f;
    protected float currentTime = 0;
    protected bool hasArrived;
    protected bool hasTrail;
    protected void BeginArcMotion(Vector3 target, float speed, float arcHeight)
    {
        arcMover = new ArcMover(transform.position, target, speed, arcHeight);
        hasCompleted = false;
        hasArrived = false;
        currentTime = 0f;
        if (tr == null)
            tr = GetComponent<TrailRenderer>();
        hasTrail = tr != null;
        Trail(true);
    }

    protected virtual void Update()
    {
        if (arcMover == null || hasCompleted || !gameObject.activeSelf) return;
        if (!hasArrived)
        {
            transform.position = arcMover.Tick(Time.deltaTime);
            if (!arcMover.IsFinished) return;
            hasArrived = true;
            Trail(false);
            EventBus.Publish(new OnArcMotionEnded(this));
            OnArcMotionCompleted();
        }
        if (!gameObject.activeSelf) return;
        if (currentTime < timeInBox)
        {
            currentTime += Time.deltaTime;
            return;
        }
        hasCompleted = true;
        ObjectPoolManager.ReturnObjectToPool(gameObject);
    }

    protected virtual void OnArcMotionCompleted() //despues mover a cada caja correspondiente
    {
        if (this is Coal coal)
        {
            EventBus.Publish(new OnCoalEarnedEvent(coal.CoalAmount));
        }
        else if (this is Coin coin)
        {
            EventBus.Publish(new OnGoldEarnedEvent(coin.CoinAmount));
        }
    }
    protected void Trail(bool isActive)
    {
        if (!hasTrail) return;
        if (isActive)
            tr.Clear();
        tr.emitting = isActive;
    }
}

public static class ArcMotionMath
{
    public static Vector3 Evaluate(Vector3 start, Vector3 end, float t, float arcHeight)
    {
        t = Mathf.Clamp01(t);
        Vector3 basePos = Vector3.Lerp(start, end, t);
        float height = Mathf.Sin(t * Mathf.PI) * arcHeight;
        return basePos + Vector3.up * height;
    }
    public static float AdvanceT(float currentT, float speed, float deltaTime, float journeyLength)
    {
        currentT += speed * deltaTime / Mathf.Max(journeyLength, 0.01f);
        return Mathf.Clamp01(currentT);
    }
}