using UnityEngine;

/// <summary>
/// Contruct and call tick to create an arc movement.
/// </summary>
public class ArcMover
{
    public Vector3 Start { get; private set; }
    public Transform Target { get; private set; }
    public float Speed { get; set; }
    public float ArcHeight { get; set; }
    public bool IsFinished { get; private set; }
    private float journeyLength;
    private float t;
    public ArcMover(Vector3 start, Transform target, float speed, float arcHeight)
    {
        Start = start;
        Target = target;
        Speed = speed;
        ArcHeight = arcHeight;
        journeyLength = Vector3.Distance(start, target.position);
        t = 0f;
        IsFinished = false;
    }

    public Vector3 Tick(float deltaTime)
    {
        if (Target == null || IsFinished) return Start;

        t = ArcMotion.AdvanceT(t, Speed, deltaTime, journeyLength);
        Vector3 pos = ArcMotion.Evaluate(Start, Target.position, t, ArcHeight);

        if (t >= 1f) IsFinished = true;

        return pos;
    }
}