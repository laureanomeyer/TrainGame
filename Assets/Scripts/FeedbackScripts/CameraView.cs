using UnityEngine;

public static class CameraView
{

    public static bool IsOutsideCamera(Vector3 worldPos, Camera cam)
    {
        Vector3 vp = cam.WorldToViewportPoint(worldPos);

        return
            vp.x < 0 || vp.x > 1 ||
            vp.y < 0 || vp.y > 1 ||
            vp.z < 0;
    }

    public static bool IsInsideCamera(Vector3 worldPos, Camera cam)
    {
        return !IsOutsideCamera(worldPos, cam);
    }
}