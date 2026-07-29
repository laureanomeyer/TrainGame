using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;

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
    private void OnDisable() => EventBus.Unsubscribe<OnWagonAddedToDisplayEvent>(OnWagonAdded);

    private void OnWagonAdded(OnWagonAddedToDisplayEvent evt)
    {
        if (!registry.TryResolveDynamic(evt.AnchorKey, out Transform target)) return;

        if(activeRoutine != null) StopCoroutine(activeRoutine);
        activeRoutine = StartCoroutine(TravelRoutine(target));
    }

    private IEnumerator TravelRoutine(Transform target)
    {
        Transform camT = travelCinemachineCamera.transform;

        camT.position = shopCinemachineCamera.transform.position;
        camT.rotation = shopCinemachineCamera.transform.rotation;

        Vector3 originPos = camT.position;
        Quaternion originRot = camT.rotation;

        Vector3 viewPos = target.position + sequence.worldOffsetFromTarget;
        Quaternion viewRot = sequence.lookAtTarget ? Quaternion.LookRotation(target.position - viewPos) : originRot;

        float distance = Vector3.Distance(originPos, viewPos); 
        float travelDuration = Mathf.Clamp(distance / sequence.travelSpeed, sequence.minTravelDuration, sequence.maxTravelDuration);

        float returnDistance = Vector3.Distance(viewPos, originPos);
        float returnDuration = Mathf.Clamp(returnDistance / sequence.travelSpeed, sequence.minTravelDuration, sequence.maxTravelDuration);

        travelCinemachineCamera.Priority = travelPriority;

        yield return Move(camT, originPos, viewPos, originRot, viewRot, travelDuration, sequence.travelCurve);
        yield return new WaitForSeconds(sequence.holdDuration);
        yield return Move(camT, viewPos, originPos, viewRot, originRot, returnDuration, sequence.returnCurve);

        travelCinemachineCamera.Priority = 0;
        activeRoutine = null;
    }

    private IEnumerator Move(Transform camT, Vector3 fromPos, Vector3 toPos, Quaternion fromRot, Quaternion toRot, float duration, AnimationCurve curve)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = curve.Evaluate(Mathf.Clamp01(elapsed / duration));
            camT.position = Vector3.Lerp(fromPos, toPos, t);
            camT.rotation = Quaternion.Slerp(fromRot, toRot, t);
            yield return null;
        }
        camT.position = toPos;
        camT.rotation = toRot;
    }
}
