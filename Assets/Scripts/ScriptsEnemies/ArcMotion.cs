using UnityEngine;

public abstract class ArcMotion : MonoBehaviour
{
    private ArcMover arcMover;
    private bool hasCompleted;

    protected void BeginArcMotion(Vector3 target, float speed, float arcHeight)
    {
        arcMover = new ArcMover(transform.position, target, speed, arcHeight);
        hasCompleted = false;
    }

    protected virtual void Update()
    {
        if (arcMover == null || hasCompleted) return;

        transform.position = arcMover.Tick(Time.deltaTime);
        if (!arcMover.IsFinished) return;

        hasCompleted = true;
        OnArcMotionCompleted();
    }

    protected virtual void OnArcMotionCompleted() { }
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