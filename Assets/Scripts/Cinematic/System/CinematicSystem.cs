using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CinematicSystem : MonoBehaviour
{
    public event Action OnCinematicFinished;

    [Header("Cinemachine Cameras")]
    [SerializeField] private CinemachineCamera gameplayCinemachineCamera;
    [SerializeField] private CinemachineCamera cinematicCinemachineCamera;

    [Header("Sequences")]
    [SerializeField] private CameraTravelSequenceSO victorySequence;
    [SerializeField] private CameraTravelSequenceSO defeatSequence;

    [Header("Target")]
    [SerializeField] private string tailAnchorKey = "TrainTail";

    [Header("Priorities")]
    [SerializeField] private int gameplayPriority = 10;
    [SerializeField] private int cinematicPriority = 20;

    private ICinematicActorRegistry registry;
    private Transform cinematicTransform;
    private Coroutine activeRoutine;
    private bool isPlaying;

    private void Awake()
    {
        registry = ServiceLocator.Get<ICinematicActorRegistry>();

        if (gameplayCinemachineCamera != null)
            gameplayCinemachineCamera.Priority = gameplayPriority;

        if (cinematicCinemachineCamera != null)
        {
            cinematicCinemachineCamera.Priority = 0;
            cinematicTransform = cinematicCinemachineCamera.transform;
        }
    }

    private void OnDestroy()
    {
        if (activeRoutine != null)
            StopCoroutine(activeRoutine);
    }

    public void CinematicPlay(RunResult result)
    {
        if (isPlaying) return;

        CameraTravelSequenceSO sequence =
            result == RunResult.Defeat ? defeatSequence : victorySequence;

        if (!TryValidate(sequence, out Transform target))
        {
            // Nunca dejamos la run colgada: si no se puede reproducir, resolvemos igual.
            OnCinematicFinished?.Invoke();
            return;
        }

        activeRoutine = StartCoroutine(CinematicRoutine(sequence, target));
    }

    private bool TryValidate(CameraTravelSequenceSO sequence, out Transform target)
    {
        target = null;

        if (gameplayCinemachineCamera == null || cinematicCinemachineCamera == null)
        {
            Debug.LogError("[CinematicSystem] Faltan asignar las Cinemachine Cameras.");
            return false;
        }

        if (sequence == null)
        {
            Debug.LogError("[CinematicSystem] Falta asignar la secuencia para este resultado.");
            return false;
        }

        if (registry != null && registry.TryResolveDynamic(tailAnchorKey, out target))
            return true;

        // Fallback mientras RunManager no registre el tail en el registry.
        if (RunManager.Instance != null && RunManager.Instance.TrainTail != null)
        {
            target = RunManager.Instance.TrainTail;
            return true;
        }

        Debug.LogError($"[CinematicSystem] No se pudo resolver el target '{tailAnchorKey}'.");
        return false;
    }

    private IEnumerator CinematicRoutine(CameraTravelSequenceSO sequence, Transform target)
    {
        isPlaying = true;

        Transform mainCameraTransform = Camera.main.transform;
        cinematicTransform.SetPositionAndRotation(
            mainCameraTransform.position,
            mainCameraTransform.rotation);

        Vector3 originPos = cinematicTransform.position;
        Quaternion originRot = cinematicTransform.rotation;

        Vector3 fixedDestination = target.position + sequence.worldOffsetFromTarget;

        Func<Vector3> destinationProvider = sequence.trackTarget
            ? () => target.position + sequence.worldOffsetFromTarget
            : () => fixedDestination;

        Func<Quaternion> rotationProvider;

        if (sequence.useFixedRotation)
        {
            Quaternion fixedRot = Quaternion.Euler(sequence.fixedEulerRotation);
            rotationProvider = () => fixedRot;
        }
        else if (sequence.lookAtTarget)
        {
            rotationProvider = () => Quaternion.LookRotation(target.position - cinematicTransform.position);
        }
        else
        {
            rotationProvider = () => originRot;
        }

        float distance = Vector3.Distance(
            originPos,
            CameraTravel.ApplyAxes(originPos, fixedDestination, sequence.travelAxes));

        float travelDuration = Mathf.Clamp(
            distance / sequence.travelSpeed,
            sequence.minTravelDuration,
            sequence.maxTravelDuration);

        cinematicCinemachineCamera.Priority = cinematicPriority;

        Action<float> fovTick = null;
        if (sequence.useFOVZoom)
        {
            float actualStartFOV = gameplayCinemachineCamera.Lens.FieldOfView; // el FOV real actual
            cinematicCinemachineCamera.Lens.FieldOfView = actualStartFOV;
            fovTick = t => cinematicCinemachineCamera.Lens.FieldOfView =
                Mathf.Lerp(actualStartFOV, sequence.endFov, t);
        }

        yield return CameraTravel.Move(
            cinematicTransform, originPos, originRot,
            destinationProvider, rotationProvider,
            travelDuration, sequence.travelCurve, sequence.travelAxes, fovTick);

        if (sequence.holdDuration > 0f)
            yield return new WaitForSeconds(sequence.holdDuration);

        if (sequence.returnToOrigin)
        {
            Vector3 currentPos = cinematicTransform.position;
            Quaternion currentRot = cinematicTransform.rotation;

            yield return CameraTravel.Move(
                cinematicTransform, currentPos, currentRot,
                () => originPos, () => originRot,
                travelDuration, sequence.returnCurve, TravelAxis.All);

            cinematicCinemachineCamera.Priority = 0;
        }

        isPlaying = false;
        activeRoutine = null;

        OnCinematicFinished?.Invoke();
    }
}