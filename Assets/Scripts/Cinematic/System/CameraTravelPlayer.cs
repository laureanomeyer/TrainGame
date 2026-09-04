using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraTravelPlayer : MonoBehaviour
{
    [Header("CinemachineCamera")]
    [SerializeField] private CinemachineCamera shopCinemachineCamera;
    [SerializeField] private CinemachineCamera travelCinemachineCamera;

    [Header("SequenceData")]
    [SerializeField] private CameraTravelSequenceSO sequence;

    [Header("Priorities")]
    [SerializeField] private int shopPriority = 10;
    [SerializeField] private int travelPriority = 20;

    private ICinematicActorRegistry registry;
    private Coroutine activeRoutine;

    private void Awake()
    {
        registry = ServiceLocator.Get<ICinematicActorRegistry>();
        shopCinemachineCamera.Priority = shopPriority;
        travelCinemachineCamera.Priority = 0;
    }

    private void OnEnable() => EventBus.Subscribe<OnWagonAddedToDisplayEvent>(OnWagonAdded);

    private void OnDisable()
    {
        EventBus.Unsubscribe<OnWagonAddedToDisplayEvent>(OnWagonAdded);
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
            travelCinemachineCamera.Priority = 0;
        }
    }

    private void OnWagonAdded(OnWagonAddedToDisplayEvent evt)
    {
        if (!registry.TryResolveDynamic(evt.AnchorKey, out Transform target)) return;
        if (activeRoutine != null) StopCoroutine(activeRoutine);
        activeRoutine = StartCoroutine(TravelRoutine(target));
    }

    private IEnumerator TravelRoutine(Transform target)
    {
        Transform camT = travelCinemachineCamera.transform;

        Transform liveCam = Camera.main.transform;
        Vector3 originPos = liveCam.position;
        Quaternion originRot = liveCam.rotation;

        camT.SetPositionAndRotation(originPos, originRot);

        Vector3 viewPos = target.position + sequence.worldOffsetFromTarget;

        Quaternion viewRot = sequence.useFixedRotation
            ? Quaternion.Euler(sequence.fixedEulerRotation)
            : (sequence.lookAtTarget
                ? Quaternion.LookRotation(target.position - viewPos)
                : originRot);

        float distance = Vector3.Distance(originPos, viewPos);

        float approachDuration = Mathf.Clamp(
            distance / sequence.travelSpeed,
            sequence.minTravelDuration,
            sequence.maxTravelDuration);

        float returnDuration = Mathf.Clamp(
            distance / sequence.returnSpeed,
            sequence.minReturnDuration,
            sequence.maxReturnDuration);

        travelCinemachineCamera.Priority = travelPriority;

        yield return CameraTravel.Move(camT, originPos, originRot,
            () => viewPos, () => viewRot,
            approachDuration, sequence.travelCurve, sequence.travelAxes);

        yield return new WaitForSeconds(sequence.holdDuration);

        yield return CameraTravel.Move(camT, viewPos, viewRot,
            () => originPos, () => originRot,
            returnDuration, sequence.returnCurve, TravelAxis.All);

        travelCinemachineCamera.Priority = 0;
        activeRoutine = null;
    }
}