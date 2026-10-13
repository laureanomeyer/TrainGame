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

        if (activeRoutine != null) StopCoroutine(activeRoutine);
        activeRoutine = null;
        travelCinemachineCamera.Priority = 0;
    }

    private void OnWagonAdded(OnWagonAddedToDisplayEvent evt)
    {
        if (!registry.TryResolveDynamic(evt.AnchorKey, out Transform target)) return;

        if (activeRoutine != null)
        {

            StopCoroutine(activeRoutine);
        }
        else
        {

            Transform live = Camera.main != null ? Camera.main.transform : shopCinemachineCamera.transform;
            travelCinemachineCamera.transform.SetPositionAndRotation(live.position, live.rotation);
            travelCinemachineCamera.Priority = travelPriority;
        }

        activeRoutine = StartCoroutine(TravelRoutine(target));
    }

    private IEnumerator TravelRoutine(Transform target)
    {
        yield return CameraTravel.PlaySequence(
            travelCinemachineCamera.transform,
            sequence,
            target,
            shopCinemachineCamera.transform);


        travelCinemachineCamera.Priority = 0;
        activeRoutine = null;
    }
}