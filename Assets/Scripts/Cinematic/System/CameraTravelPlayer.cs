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

        camT.SetPositionAndRotation(
            shopCinemachineCamera.transform.position,
            shopCinemachineCamera.transform.rotation);

        Vector3 originPos = camT.position;
        Quaternion originRot = camT.rotation;

        // target.position ya es la posición donde se agregó el wagon nuevo (headPos)
        Vector3 viewPos = target.position + sequence.worldOffsetFromTarget;
        Quaternion viewRot = sequence.lookAtTarget
            ? Quaternion.LookRotation(target.position - viewPos)
            : originRot;

        float cameraDuration = Mathf.Clamp(
            Vector3.Distance(originPos, viewPos) / sequence.travelSpeed,
            sequence.minTravelDuration,
            sequence.maxTravelDuration);

        travelCinemachineCamera.Priority = travelPriority;

        yield return CameraTravel.Move(camT, originPos, originRot,
            () => viewPos, () => viewRot,
            cameraDuration, sequence.travelCurve, sequence.travelAxes);

        yield return new WaitForSeconds(sequence.holdDuration);

        yield return CameraTravel.Move(camT, viewPos, viewRot,
            () => originPos, () => originRot,
            cameraDuration, sequence.returnCurve, TravelAxis.All);

        travelCinemachineCamera.Priority = 0;
        activeRoutine = null;
    }
}