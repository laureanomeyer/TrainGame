using UnityEngine;

public static class ArcMotion
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