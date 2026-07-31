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

    public static IEnumerator Move(
        Transform camT,
        Vector3 fromPos,
        Quaternion fromRot,
        Func<Vector3> destinationProvider,
        Func<Quaternion> rotationProvider,
        float duration,
        AnimationCurve curve,
        TravelAxis axes)
    {
        Vector3 destination = ApplyAxes(fromPos, destinationProvider(), axes);
        Quaternion destinationRot = rotationProvider();

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = curve.Evaluate(Mathf.Clamp01(elapsed / duration));

            destination = ApplyAxes(fromPos, destinationProvider(), axes);
            destinationRot = rotationProvider();

            camT.SetPositionAndRotation(
                Vector3.Lerp(fromPos, destination, t),
                Quaternion.Slerp(fromRot, destinationRot, t)
            );

            yield return null;
        }

        camT.SetPositionAndRotation(destination, destinationRot);
    }
}