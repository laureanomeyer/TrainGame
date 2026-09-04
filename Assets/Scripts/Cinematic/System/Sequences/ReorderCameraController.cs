using Unity.Cinemachine;
using UnityEngine;

public class ReorderCameraController : MonoBehaviour
{
    [Header("CinemachineCamera")]
    [SerializeField] private CinemachineCamera reorderCinemachineCamera;

    [Header("Priorities")]
    [SerializeField] private int activePriority = 20;
    [SerializeField] private int inactivePriority = 0;

    [Header("Framing")]
    [Tooltip("Offset en espacio del MUNDO desde el wagon hovereado (ej: 0,5,0 para quedar arriba).")]
    [SerializeField] private Vector3 topOffset = new Vector3(0f, 5f, 0f);

    [Tooltip("Rotación fija de la cámara en euler. No mira exacto al target, mantiene este ángulo siempre.")]
    [SerializeField] private Vector3 fixedEulerRotation = new Vector3(35f, 0f, 0f);

    [Header("Follow Smoothing")]
    [Tooltip("Tiempo aproximado (seg) que tarda en alcanzar al target. Más bajo = más rápido/rígido.")]
    [SerializeField] private float positionSmoothTime = 0.25f;
    [SerializeField] private float rotationSpeed = 8f;

    private Transform currentTarget;
    private Vector3 velocity;
    private bool isActive;

    private void Awake()
    {
        reorderCinemachineCamera.Priority = inactivePriority;
        ServiceLocator.Register(this);
    }

    public void Activate(Transform initialTarget)
    {
        currentTarget = initialTarget;
        isActive = true;

        if (currentTarget != null)
        {
            Transform camT = reorderCinemachineCamera.transform;
            Vector3 snapPos = currentTarget.position + topOffset;
            camT.position = snapPos;
            camT.rotation = ComputeRotation();
            velocity = Vector3.zero;
        }

        reorderCinemachineCamera.Priority = activePriority;
    }

    public void Deactivate()
    {
        isActive = false;
        currentTarget = null;
        reorderCinemachineCamera.Priority = inactivePriority;
    }

    public void SetTarget(Transform target)
    {
        currentTarget = target;
    }

    private void LateUpdate()
    {
        if (!isActive || currentTarget == null) return;

        Transform camT = reorderCinemachineCamera.transform;
        Vector3 desiredPos = currentTarget.position + topOffset;

        camT.position = Vector3.SmoothDamp(camT.position, desiredPos, ref velocity, positionSmoothTime);

        Quaternion desiredRot = ComputeRotation();
        camT.rotation = Quaternion.Slerp(camT.rotation, desiredRot, rotationSpeed * Time.deltaTime);
    }

    private Quaternion ComputeRotation()
    {
        return Quaternion.Euler(fixedEulerRotation);
    }
}