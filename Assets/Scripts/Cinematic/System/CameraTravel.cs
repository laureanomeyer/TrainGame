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


    public static bool TryGetRendererBounds(Transform target, out Bounds bounds)
    {
        bounds = default;
        if (target == null) return false;

        bool hasBounds = false;
        foreach (Renderer r in target.GetComponentsInChildren<Renderer>())
        {
            if (!r.enabled || r is ParticleSystemRenderer || r is TrailRenderer || r is LineRenderer)
                continue;

            if (!hasBounds) { bounds = r.bounds; hasBounds = true; }
            else bounds.Encapsulate(r.bounds);
        }

        return hasBounds;
    }

    public static Vector3 ComputeLocalAnchor(Transform target)
    {
        if (!TryGetRendererBounds(target, out Bounds bounds))
        {
            if (target != null)
                Debug.LogWarning($"[CameraTravel] '{target.name}' no tiene renderers; se usa el pivot como ancla.");
            return Vector3.zero;
        }

        return Quaternion.Inverse(target.rotation) * (bounds.center - target.position);
    }


    public static Vector3 ResolvePoint(Transform target, Vector3 localAnchor, Vector3 offset, OffsetSpace space)
    {
        Vector3 anchor = target.position + target.rotation * localAnchor;
        return space == OffsetSpace.Target
            ? anchor + target.rotation * offset
            : anchor + offset;
    }

    public static Vector3 ResolvePoint(Transform target, Vector3 offset, OffsetSpace space)
        => ResolvePoint(target, Vector3.zero, offset, space);

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
                    Vector3.LerpUnclamped(fromPos, destination, t),
                    Quaternion.SlerpUnclamped(fromRot, destinationRot, t));

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


        Vector3 localAnchor = seq.anchorToBoundsCenter ? ComputeLocalAnchor(target) : Vector3.zero;

        Vector3 fixedDest = ResolvePoint(target, localAnchor, seq.offsetFromTarget, seq.offsetSpace);
        Vector3 fixedLook = ResolvePoint(target, localAnchor, seq.lookAtOffset, seq.offsetSpace);

        Func<Vector3> destination = seq.trackTarget
            ? () => target != null ? ResolvePoint(target, localAnchor, seq.offsetFromTarget, seq.offsetSpace) : fixedDest
            : () => fixedDest;

        Func<Vector3> lookPoint = seq.trackTarget
            ? () => target != null ? ResolvePoint(target, localAnchor, seq.lookAtOffset, seq.offsetSpace) : fixedLook
            : () => fixedLook;


        Func<Vector3> finalCamPos = () => ApplyAxes(fromPos, destination(), seq.travelAxes);
        Func<Quaternion> rotation = BuildRotation(seq, finalCamPos, lookPoint, fromRot);

        float approachDistance = Vector3.Distance(fromPos, finalCamPos());
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
        CameraTravelSequenceSO seq,
        Func<Vector3> camPosition,
        Func<Vector3> lookPoint,
        Quaternion fallback)
    {
        if (seq.useFixedRotation)
        {
            Quaternion fixedRot = Quaternion.Euler(seq.fixedEulerRotation);
            return () => fixedRot;
        }

        if (seq.lookAtTarget)
        {
            Quaternion last = fallback;
            return () =>
            {
                Vector3 dir = lookPoint() - camPosition();
                if (dir.sqrMagnitude > 0.0001f)
                    last = Quaternion.LookRotation(dir, Vector3.up);
                return last;
            };
        }

        return () => fallback;
    }
}