using System.Collections;
using UnityEngine;

public class WagonArrivalAnimator : MonoBehaviour
{
    private Coroutine activeRoutine;

    public void BeginArrival(Vector3 worldOffset, float duration, AnimationCurve curve)
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
        }
        activeRoutine = StartCoroutine(ArrivalRoutine(worldOffset, duration, curve));
    }

    private IEnumerator ArrivalRoutine(Vector3 worldOffset, float duration, AnimationCurve curve)
    {
        Vector3 finalPos = transform.position;
        Vector3 startPos = finalPos + worldOffset;
        transform.position = startPos;

        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float k = curve.Evaluate(Mathf.Clamp01(t / duration));
            transform.position = Vector3.LerpUnclamped(startPos, finalPos, k);
            yield return null;
        }

        transform.position = finalPos;
        activeRoutine = null;

    }
}
