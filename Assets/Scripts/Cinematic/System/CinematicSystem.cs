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
    [SerializeField] private string locomotiveAnchorKey = "Locomotive";

    [Header("Priorities")]
    [SerializeField] private int gameplayPriority = 10;
    [SerializeField] private int cinematicPriority = 20;

    [Header("Victory Threshold")]
    [SerializeField] private Transform victoryThreshold;
    [SerializeField] private float thresholdTailOffset = 200f;
    [SerializeField] private float thresholdRetreatDuration = 2f;
    [Tooltip("Scripts que mueven el threshold durante la run. Se desactivan al empezar la cinemática de victoria.")]
    [SerializeField] private Behaviour[] thresholdMoversToDisable;

    private ICinematicActorRegistry registry;
    private bool isPlaying;

    private void Awake()
    {
        registry = ServiceLocator.Get<ICinematicActorRegistry>();

        if (gameplayCinemachineCamera != null)
            gameplayCinemachineCamera.Priority = gameplayPriority;

        if (cinematicCinemachineCamera != null)
            cinematicCinemachineCamera.Priority = 0;
    }

    public void CinematicPlay(RunResult result)
    {
        if (isPlaying) return;

        CameraTravelSequenceSO sequence = result == RunResult.Defeat ? defeatSequence : victorySequence;
        string anchorKey = result == RunResult.Defeat ? locomotiveAnchorKey : tailAnchorKey;

        if (!TryValidate(sequence, anchorKey, out Transform target))
        {
            OnCinematicFinished?.Invoke();
            return;
        }

        StartCoroutine(CinematicRoutine(sequence, target, result));
    }

    private bool TryValidate(CameraTravelSequenceSO sequence, string anchorKey, out Transform target)
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

        if (registry != null && registry.TryResolveDynamic(anchorKey, out target))
            return true;

        if (anchorKey == tailAnchorKey && RunManager.Instance != null && RunManager.Instance.TrainTail != null)
        {
            target = RunManager.Instance.TrainTail;
            return true;
        }

        Debug.LogError($"[CinematicSystem] No se pudo resolver el target '{anchorKey}'.");
        return false;
    }

    private IEnumerator CinematicRoutine(CameraTravelSequenceSO sequence, Transform target, RunResult result)
    {
        isPlaying = true;

        if (result == RunResult.Victory && victoryThreshold != null)
            StartCoroutine(ThresholdRetreat(target));

        Transform camT = cinematicCinemachineCamera.transform;
        Transform live = Camera.main != null ? Camera.main.transform : gameplayCinemachineCamera.transform;
        camT.SetPositionAndRotation(live.position, live.rotation);


        float gameplayFov = gameplayCinemachineCamera.Lens.FieldOfView;
        cinematicCinemachineCamera.Lens.FieldOfView = gameplayFov;

        Action<float> approachTick = null;
        Action<float> returnTick = null;

        if (sequence.useFOVZoom)
        {
            approachTick = t => cinematicCinemachineCamera.Lens.FieldOfView =
                Mathf.Lerp(gameplayFov, sequence.endFov, t);

            returnTick = t => cinematicCinemachineCamera.Lens.FieldOfView =
                Mathf.Lerp(sequence.endFov, gameplayCinemachineCamera.Lens.FieldOfView, t);
        }

        cinematicCinemachineCamera.Priority = cinematicPriority;

        yield return CameraTravel.PlaySequence(
            camT, sequence, target, gameplayCinemachineCamera.transform, approachTick, returnTick);

        if (sequence.returnToOrigin)
            cinematicCinemachineCamera.Priority = 0;

        isPlaying = false;
        OnCinematicFinished?.Invoke();
    }

    private IEnumerator ThresholdRetreat(Transform tailTarget)
    {

        foreach (var mover in thresholdMoversToDisable)
            if (mover != null) mover.enabled = false;

        Vector3 startPos = victoryThreshold.position;
        float timer = 0f;

        while (tailTarget != null)
        {
            timer += Time.deltaTime;
            float t = thresholdRetreatDuration > 0f ? Mathf.Clamp01(timer / thresholdRetreatDuration) : 1f;

            Vector3 targetPos = new(tailTarget.position.x - thresholdTailOffset, startPos.y, startPos.z);
            victoryThreshold.position = Vector3.Lerp(startPos, targetPos, t);

            yield return null;
        }
    }
}