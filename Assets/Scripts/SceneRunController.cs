using UnityEngine;

public class SceneRunController : MonoBehaviour
{
    [Header("Scene duration")]
    private float sceneDuration;

    private float currentTime;
    private bool runFinished;
    private bool runStarted = true;

    [Header("Cinematic")]
    [SerializeField] private CinematicSystem cinematicSystem;

    public float Progress => 1f - Mathf.Clamp01(currentTime / sceneDuration);

    private void Awake()
    {
        var sessionConfig = ServiceLocator.Get<SessionConfig>();
        sceneDuration = sessionConfig.RunDurantion;
        currentTime = sceneDuration;
        runFinished = false;

        TutorialEvents.OnSetTimerStarted += SetRunStarted;

        if (cinematicSystem != null)
            cinematicSystem.OnCinematicFinished += FinishRun;
    }

    private void Update()
    {
        if (runFinished) return;
        if (!GameManager.Instance.IsGameplayState) return;

        if (runStarted)
            currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            runFinished = true;

            if (cinematicSystem == null)
            {
                Debug.LogError("CinematicSystem no está asignado.");
                return;
            }

            cinematicSystem.CinematicPlay();
        }
    }

    private void FinishRun()
    {
        GameManager.Instance.GoToStore();
    }

    private void SetRunStarted(bool runStarted)
    {
        this.runStarted = runStarted;
    }

    private void OnDestroy()
    {
        TutorialEvents.OnSetTimerStarted -= SetRunStarted;

        if (cinematicSystem != null)
            cinematicSystem.OnCinematicFinished -= FinishRun;
    }
}