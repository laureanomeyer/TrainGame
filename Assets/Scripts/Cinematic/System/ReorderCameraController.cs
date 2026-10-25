using Unity.Cinemachine;
using UnityEngine;

public class ReorderCameraController : MonoBehaviour
{
    [Header("CinemachineCamera")]
    [SerializeField] private CinemachineCamera reorderCinemachineCamera;

    [Header("Priorities")]
    [SerializeField] private int activePriority = 30;
    [SerializeField] private int inactivePriority = 0;

    [Header("Framing")]
    [Tooltip("Offset en espacio del MUNDO desde el CENTRO del wagon hovereado.")]
    [SerializeField] private Vector3 topOffset = new Vector3(0f, 30f, -15f);

    [Tooltip("Rotación fija de la cámara en euler.")]
    [SerializeField] private Vector3 fixedEulerRotation = new Vector3(50f, 0f, 0f);

    [Header("Follow Smoothing")]
    [Tooltip("Tiempo aproximado (seg) que tarda en alcanzar al target. Más bajo = más rápido/rígido.")]
    [SerializeField] private float positionSmoothTime = 0.25f;
    [SerializeField] private float rotationSpeed = 8f;

    private Transform currentTarget;
    private Vector3 focusOffset;  
    private Vector3 velocity;
    private bool isActive;

    private void Awake()
    {
        reorderCinemachineCamera.Priority = inactivePriority;
        ServiceLocator.Register(this);
    }

    private void OnDestroy() => ServiceLocator.Unregister<ReorderCameraController>();

    public void Activate(Transform initialTarget)
    {
        SetTarget(initialTarget);
        isActive = true;

        if (currentTarget != null)
        {
            Transform camT = reorderCinemachineCamera.transform;
            camT.SetPositionAndRotation(FocusPoint() + topOffset, Quaternion.Euler(fixedEulerRotation));
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
        focusOffset = ComputeFocusOffset(target);
    }

    private void LateUpdate()
    {
        if (!isActive || currentTarget == null) return;

        Transform camT = reorderCinemachineCamera.transform;
        Vector3 desiredPos = FocusPoint() + topOffset;

        camT.position = Vector3.SmoothDamp(camT.position, desiredPos, ref velocity, positionSmoothTime);

        Quaternion desiredRot = Quaternion.Euler(fixedEulerRotation);
        camT.rotation = Quaternion.Slerp(camT.rotation, desiredRot, rotationSpeed * Time.deltaTime);
    }


    private Vector3 FocusPoint() => currentTarget.position + currentTarget.rotation * focusOffset;


    private static Vector3 ComputeFocusOffset(Transform target)
    {
        if (target == null) return Vector3.zero;

        var data = target.GetComponent<ShopWagonData>();
        if (data == null) return Vector3.zero;

        Vector3 worldOffset = data.GetWorldBounds().center - target.position;
        return Quaternion.Inverse(target.rotation) * worldOffset;
    }
}