using UnityEngine;

public class SceneRunController : MonoBehaviour
{
    private float sceneDuration;
    private float currentTime;
    private bool runFinished;
    private bool runStarted = true;
    private RunResult pendingResult = RunResult.None;

    [Header("Cinematic")]
    [SerializeField] private CinematicSystem cinematicSystem;

    public float Progress => 1f - Mathf.Clamp01(currentTime / sceneDuration);

    private void Awake()
    {
        var sessionConfig = ServiceLocator.Get<SessionConfig>();
        sceneDuration = sessionConfig.RunDurantion;
        currentTime = sceneDuration;
        runFinished = false;

        EventBus.Subscribe<OnSetTimerStartedEvent>(CallSetRunStartedEvent);
        EventBus.Subscribe<OnRunEndedEvent>(CallRunEndedEvent);

        if (cinematicSystem != null)
            cinematicSystem.OnCinematicFinished += FinishRun;
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<OnSetTimerStartedEvent>(CallSetRunStartedEvent);
        EventBus.Unsubscribe<OnRunEndedEvent>(CallRunEndedEvent);

        if (cinematicSystem != null)
            cinematicSystem.OnCinematicFinished -= FinishRun;
    }

    private void Update()
    {
        if (runFinished) return;
        if (!GameManager.Instance.IsGameplayState) return;

        if (runStarted)
            currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
            EndRun(RunResult.Victory);
    }

    private void CallRunEndedEvent(OnRunEndedEvent runEndedEvent)
    {
        EndRun(runEndedEvent.Result);
    }

    private void EndRun(RunResult result)
    {
        if (runFinished) return;
        if (result == RunResult.None) return;

        runFinished = true;
        pendingResult = result;
        currentTime = 0f;

        EventBus.Publish(new OnSetTimerStartedEvent(false));

        if (cinematicSystem == null)
        {
            Debug.LogError("[SceneRunController] CinematicSystem no asignado. Resuelvo sin cinemática.");
            FinishRun();
            return;
        }

        cinematicSystem.CinematicPlay(result);
    }

    private void FinishRun()
    {
        if (pendingResult == RunResult.Defeat)
        {
            GameManager.Instance.Defeat();
            return;
        }

        if (GameManager.Instance.IsFinalStation())
            GameManager.Instance.Victory();
        else
            GameManager.Instance.GoToStore();
    }

    private void CallSetRunStartedEvent(OnSetTimerStartedEvent startTimerEvent)
    {
        SetRunStarted(startTimerEvent.Can);
    }

    private void SetRunStarted(bool runStarted)
    {
        this.runStarted = runStarted;
    }
}