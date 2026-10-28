using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;

public class CinematicSystem : MonoBehaviour
{
    public event Action OnCinematicFinished;

    [Header("Cinemachine Cameras")]
    [SerializeField] private CinemachineCamera gameplayCinemachineCamera;
    [SerializeField] private CinemachineCamera cinematicCinemachineCamera;

    [Header("Sequences")]
    [Tooltip("Solo se usa en la derrota. La victoria no hace travel: la cámara queda donde estaba.")]
    [SerializeField] private CameraTravelSequenceSO defeatSequence;

    [Header("Target")]
    [SerializeField] private string locomotiveAnchorKey = "Locomotive";

    [Header("Priorities")]
    [SerializeField] private int gameplayPriority = 10;
    [SerializeField] private int cinematicPriority = 20;

    [Header("Level End (Victory)")]
    [Tooltip("Dirección en la que avanza el tren. Los tiles se mueven en sentido contrario.")]
    [SerializeField] private Vector3 trainForward = Vector3.right;
    [Tooltip("Distancia antes de la trompa a la que el mapa empieza a frenar y el tren a avanzar. 0 = justo en la trompa.")]
    [SerializeField] private float transferLeadDistance = 0f;
    [Tooltip("Segundos en los que el mapa frena y el tren acelera (movimiento relativo constante).")]
    [SerializeField] private float transferDuration = 0.5f;
    [Tooltip("Segundos que se sigue mostrando la entrada después de que la locomotora desaparece.")]
    [SerializeField] private float holdAfterLocomotiveHidden = 1f;
    [Tooltip("Desliza la cámara solo sobre el eje de la vía (sin rotar ni cambiar altura) para centrar la boca.")]
    [SerializeField] private bool centerEntranceOnScreen = true;
    [SerializeField] private float centerPanDuration = 0.8f;
    [Tooltip("Si la montaña no llega en este tiempo, la cinemática termina igual.")]
    [SerializeField] private float levelEndTimeout = 20f;

    [Header("Victory Threshold")]
    [SerializeField] private Transform victoryThreshold;
    [SerializeField] private float thresholdTailOffset = 200f;
    [SerializeField] private float thresholdRetreatDuration = 2f;
    [Tooltip("Scripts que mueven el threshold durante la run. Se desactivan al empezar la cinemática de victoria.")]
    [SerializeField] private Behaviour[] thresholdMoversToDisable;

    [Tooltip("Aleja la cámara sobre su eje de visión (sin rotar). Sube y amplía el encuadre manteniendo centrada la boca.")]
    [SerializeField] private float cameraPullBack = 15f;
    [Tooltip("Altura extra en Y del mundo. Sube la cámara, la cueva baja en pantalla.")]
    [SerializeField] private float cameraExtraHeight = 0f;

    private ICinematicActorRegistry registry;
    private bool isPlaying;
    private bool thresholdVisibilityApplied;

    private void Awake()
    {
        registry = ServiceLocator.Get<ICinematicActorRegistry>();

        if (gameplayCinemachineCamera != null)
            gameplayCinemachineCamera.Priority = gameplayPriority;

        if (cinematicCinemachineCamera != null)
            cinematicCinemachineCamera.Priority = 0;
    }

    private void Update()
    {
        if (thresholdVisibilityApplied || victoryThreshold == null) return;

        MapManager map = RunManager.Instance != null ? RunManager.Instance.MapManager : null;
        if (map == null || !map.IsInitialized) return;

        thresholdVisibilityApplied = true;
        victoryThreshold.gameObject.SetActive(map.ShowVictoryThreshold);
    }

    public void CinematicPlay(RunResult result)
    {
        if (isPlaying) return;

        bool victory = result == RunResult.Victory;

        if (!CamerasAssigned() ||
            (!victory && defeatSequence == null) ||
            !TryResolveLocomotive(out Transform locomotive))
        {
            if (!victory && defeatSequence == null)
                Debug.LogError("[CinematicSystem] Falta asignar la secuencia de derrota.");

            OnCinematicFinished?.Invoke();
            return;
        }

        if (victory) StartCoroutine(LevelEndRoutine(locomotive));
        else StartCoroutine(DefeatRoutine(defeatSequence, locomotive));
    }

    private bool CamerasAssigned()
    {
        if (gameplayCinemachineCamera != null && cinematicCinemachineCamera != null) return true;
        Debug.LogError("[CinematicSystem] Faltan asignar las Cinemachine Cameras.");
        return false;
    }

    private bool TryResolveLocomotive(out Transform locomotive)
    {
        if (registry != null && registry.TryResolveDynamic(locomotiveAnchorKey, out locomotive))
            return true;

        locomotive = RunManager.Instance != null && RunManager.Instance.LocomotiveBrain != null
            ? RunManager.Instance.LocomotiveBrain.transform
            : null;

        if (locomotive == null)
            Debug.LogError($"[CinematicSystem] No se pudo resolver la locomotora ('{locomotiveAnchorKey}').");

        return locomotive != null;
    }

    private Transform TakeOverCamera(out float gameplayFov)
    {
        Transform camT = cinematicCinemachineCamera.transform;
        Transform live = Camera.main != null ? Camera.main.transform : gameplayCinemachineCamera.transform;
        camT.SetPositionAndRotation(live.position, live.rotation);

        gameplayFov = gameplayCinemachineCamera.Lens.FieldOfView;
        cinematicCinemachineCamera.Lens.FieldOfView = gameplayFov;
        cinematicCinemachineCamera.Priority = cinematicPriority;

        return camT;
    }



    private IEnumerator DefeatRoutine(CameraTravelSequenceSO sequence, Transform target)
    {
        isPlaying = true;

        Transform camT = TakeOverCamera(out float gameplayFov);

        Action<float> approachTick = null;
        Action<float> returnTick = null;

        if (sequence.useFOVZoom)
        {
            approachTick = t => cinematicCinemachineCamera.Lens.FieldOfView = Mathf.Lerp(gameplayFov, sequence.endFov, t);
            returnTick = t => cinematicCinemachineCamera.Lens.FieldOfView =
                Mathf.Lerp(sequence.endFov, gameplayCinemachineCamera.Lens.FieldOfView, t);
        }

        yield return CameraTravel.PlaySequence(
            camT, sequence, target, gameplayCinemachineCamera.transform, approachTick, returnTick);

        if (sequence.returnToOrigin)
            cinematicCinemachineCamera.Priority = 0;
        else if (sequence.trackTarget)
            StartCoroutine(KeepTracking(camT, target, sequence));

        Finish();
    }



    private IEnumerator LevelEndRoutine(Transform locomotive)
    {
        isPlaying = true;

        RunManager run = RunManager.Instance;
        MapManager map = run != null ? run.MapManager : null;

        if (victoryThreshold != null && victoryThreshold.gameObject.activeInHierarchy &&
            run != null && run.TrainTail != null)
            StartCoroutine(ThresholdRetreat(run.TrainTail));


        Transform camT = TakeOverCamera(out _);

        
        gameplayCinemachineCamera.Follow = null;
        gameplayCinemachineCamera.LookAt = null;

        Vector3 fwd = trainForward.sqrMagnitude > 0f ? trainForward.normalized : Vector3.right;

        GetEdges(locomotive, fwd, out float locoFront, out float locoRear);
        float locoAlong = Along(locomotive.position, fwd);
        float frontOffset = locoFront - locoAlong;
        float rearOffset = locoRear - locoAlong;


        Transform entrance = null;
        float waited = 0f;

        while (waited < levelEndTimeout && locomotive != null)
        {
            if (entrance == null)
                registry?.TryResolveDynamic(LevelEndTile.EntranceAnchorKey, out entrance);

            if (entrance != null &&
                Along(entrance.position, fwd) <= Along(locomotive.position, fwd) + frontOffset + transferLeadDistance)
                break;

            waited += Time.deltaTime;
            yield return null;
        }

        if (entrance == null || locomotive == null)
        {
            Debug.LogWarning("[CinematicSystem] No llegó la entrada de fin de nivel. Termino sin montaña.");
            Finish();
            yield break;
        }


        List<Transform> trainRoots = CollectTrainRoots(locomotive);
        float speed = run != null ? run.TrainSpeed : 0f;

        Vector3 camStart = camT.position;
        float elapsed = 0f;
        float hiddenTime = 0f;
        bool notified = false;


        while (locomotive != null)
        {
            float dt = Time.deltaTime;
            elapsed += dt;

            float k = transferDuration > 0f ? Mathf.SmoothStep(0f, 1f, elapsed / transferDuration) : 1f;

            map?.SetScrollMultiplier(1f - k);

            Vector3 step = fwd * (speed * k * dt);
            foreach (Transform part in trainRoots)
                if (part != null) part.position += step;

            float p = centerPanDuration > 0f ? Mathf.SmoothStep(0f, 1f, elapsed / centerPanDuration) : 1f;

            Vector3 pan = centerEntranceOnScreen
                ? fwd * PanToCenter(camStart, camT.rotation, entrance.position, fwd)
                : Vector3.zero;

            Vector3 lift = -(camT.rotation * Vector3.forward) * cameraPullBack + Vector3.up * cameraExtraHeight;

            camT.position = camStart + (pan + lift) * p;


            if (!notified && Along(locomotive.position, fwd) + rearOffset >= Along(entrance.position, fwd))
            {
                hiddenTime += dt;
                if (hiddenTime >= holdAfterLocomotiveHidden)
                {
                    notified = true;
                    Finish();
                }
            }

            yield return null;
        }

        if (!notified) Finish();


    }


    private static List<Transform> CollectTrainRoots(Transform locomotive)
    {
        var candidates = new List<Transform> { locomotive };
        RunManager run = RunManager.Instance;

        if (run != null)
        {
            foreach (IWagon wagon in run.ActiveWagons)
                if (wagon is Component c && c != null) candidates.Add(c.transform);

            if (run.TrainTail != null) candidates.Add(run.TrainTail);
        }

        candidates = candidates.Where(t => t != null).Distinct().ToList();
        return candidates.Where(t => !candidates.Any(o => o != t && t.IsChildOf(o))).ToList();
    }

 
    private static float PanToCenter(Vector3 camPos, Quaternion camRot, Vector3 point, Vector3 fwd)
    {
        Vector3 right = camRot * Vector3.right;
        float denom = Vector3.Dot(fwd, right);
        if (Mathf.Abs(denom) < 0.2f) return 0f; 

        return Vector3.Dot(point - camPos, right) / denom;
    }

    private void Finish()
    {
        isPlaying = false;
        OnCinematicFinished?.Invoke();
    }

    private static float Along(Vector3 point, Vector3 fwd) => Vector3.Dot(point, fwd);

    private static void GetEdges(Transform t, Vector3 fwd, out float front, out float rear)
    {
        if (!CameraTravel.TryGetRendererBounds(t, out Bounds b))
        {
            front = rear = Along(t.position, fwd);
            return;
        }

        float center = Vector3.Dot(b.center, fwd);
        float extent = Vector3.Dot(b.extents, new Vector3(Mathf.Abs(fwd.x), Mathf.Abs(fwd.y), Mathf.Abs(fwd.z)));
        front = center + extent;
        rear = center - extent;
    }


    private IEnumerator ThresholdRetreat(Transform tailTarget)
    {
        foreach (var mover in thresholdMoversToDisable)
            if (mover != null) mover.enabled = false;

        Vector3 startPos = victoryThreshold.position;


        Vector3 targetPos = new(tailTarget.position.x - thresholdTailOffset, startPos.y, startPos.z);

        float timer = 0f;
        while (timer < thresholdRetreatDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / thresholdRetreatDuration);
            victoryThreshold.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        victoryThreshold.position = targetPos;
    }

    private IEnumerator KeepTracking(Transform camT, Transform target, CameraTravelSequenceSO sequence)
    {
        Vector3 localAnchor = sequence.anchorToBoundsCenter ? CameraTravel.ComputeLocalAnchor(target) : Vector3.zero;

        while (target != null)
        {
            Vector3 desired = CameraTravel.ResolvePoint(target, localAnchor, sequence.offsetFromTarget, sequence.offsetSpace);
            camT.position = CameraTravel.ApplyAxes(camT.position, desired, sequence.travelAxes);
            yield return null;
        }
    }
}