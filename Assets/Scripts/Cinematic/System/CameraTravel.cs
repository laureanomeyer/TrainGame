using System;
using System.Collections;
using UnityEngine;

public static class CameraTravel
{
    public static Vector3 ApplyAxes(Vector3 origin, Vector3 destination, TravelAxis axes)
    {
        return new Vector3(
            (axes & TravelAxis.X) != 0 ? destination.x : origin.x,
            (axes & TravelAxis.Y) != 0 ? destination.y : origin.y,
            (axes & TravelAxis.Z) != 0 ? destination.z : origin.z
        );
    }

    public static float Duration(float distance, float speed, float min, float max)
    {
        if (speed <= 0f) return max;
        return Mathf.Clamp(distance / speed, min, max);
    }

    public static IEnumerator Move(
        Transform camT,
        Vector3 fromPos,
        Quaternion fromRot,
        Func<Vector3> destinationProvider,
        Func<Quaternion> rotationProvider,
        float duration,
        AnimationCurve curve,
        TravelAxis axes,
        Action<float> onTick = null)
    {
        if (duration > 0f)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                float raw = Mathf.Clamp01(elapsed / duration);
                float t = curve != null ? curve.Evaluate(raw) : raw;

                Vector3 destination = ApplyAxes(fromPos, destinationProvider(), axes);
                Quaternion destinationRot = rotationProvider();

                camT.SetPositionAndRotation(
                    Vector3.Lerp(fromPos, destination, t),
                    Quaternion.Slerp(fromRot, destinationRot, t));

                onTick?.Invoke(t);
                yield return null;
            }
        }

        camT.SetPositionAndRotation(ApplyAxes(fromPos, destinationProvider(), axes), rotationProvider());
        onTick?.Invoke(1f);
    }

    public static IEnumerator PlaySequence(
        Transform camT,
        CameraTravelSequenceSO seq,
        Transform target,
        Transform returnTo,
        Action<float> approachTick = null,
        Action<float> returnTick = null)
    {
        Vector3 fromPos = camT.position;
        Quaternion fromRot = camT.rotation;

        Vector3 fixedDest = target.position + seq.worldOffsetFromTarget;

        Func<Vector3> destination = seq.trackTarget
            ? () => target != null ? target.position + seq.worldOffsetFromTarget : fixedDest
            : () => fixedDest;

        Func<Quaternion> rotation = BuildRotation(seq, target, camT, fromRot);

        float approachDistance = Vector3.Distance(fromPos, ApplyAxes(fromPos, fixedDest, seq.travelAxes));
        float approachDuration = Duration(approachDistance, seq.travelSpeed, seq.minTravelDuration, seq.maxTravelDuration);

        yield return Move(camT, fromPos, fromRot, destination, rotation,
            approachDuration, seq.travelCurve, seq.travelAxes, approachTick);

        if (seq.holdDuration > 0f)
            yield return new WaitForSeconds(seq.holdDuration);

        if (!seq.returnToOrigin || returnTo == null) yield break;

        Vector3 holdPos = camT.position;
        Quaternion holdRot = camT.rotation;

        float returnDistance = Vector3.Distance(holdPos, returnTo.position);
        float returnDuration = Duration(returnDistance, seq.returnSpeed, seq.minReturnDuration, seq.maxReturnDuration);


        yield return Move(camT, holdPos, holdRot,
            () => returnTo.position, () => returnTo.rotation,
            returnDuration, seq.returnCurve, TravelAxis.All, returnTick);
    }

    private static Func<Quaternion> BuildRotation(
        CameraTravelSequenceSO seq, Transform target, Transform camT, Quaternion fallback)
    {
        if (seq.useFixedRotation)
        {
            Quaternion fixedRot = Quaternion.Euler(seq.fixedEulerRotation);
            return () => fixedRot;
        }

        if (seq.lookAtTarget)
        {
            return () =>
            {
                if (target == null) return camT.rotation;
                Vector3 dir = target.position - camT.position;
                return dir.sqrMagnitude > 0.0001f ? Quaternion.LookRotation(dir) : camT.rotation;
            };
        }

        return () => fallback;
    }
}