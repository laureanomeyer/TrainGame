using UnityEngine;

public class SceneRunController : MonoBehaviour
{
    [Header("Scene duration")]
    private float sceneDuration;

    private float currentTime;
    private bool runFinished;
    private bool runStarted = true;

    [Header("Cinematic")]
    [SerializeField] private Cinematic victoryCinematic;

    public float Progress => 1f - Mathf.Clamp01(currentTime / sceneDuration);

    private void Awake()
    {
        if (victoryCinematic == null)
        {
            victoryCinematic = FindFirstObjectByType<Cinematic>(FindObjectsInactive.Include);
        }

        sceneDuration = GameManager.Instance.Session.SessionConfig.RunDurantion;
        currentTime = sceneDuration;
        runFinished = false;

        TutorialEvents.OnSetTimerStarted += SetRunStarted;
    }

    void Update()
    {
        if (runFinished) return;
        if (!GameManager.Instance.IsGameplayState) return;

        if (runStarted)
            currentTime -= Time.deltaTime;

        if (currentTime < 0)
        {
            runFinished = true;

            if (victoryCinematic == null)
            {
                Debug.LogError("Victory Cinematic no está asignado en " + gameObject.name);
                return;
            }

            // Salgo del GameplayState antes de la cinemática.
            GameManager.Instance.EnterTransitionState();

            // Ahora empieza la cinemática.
            victoryCinematic.PlayCinematic();
        }
    }

    void SetRunStarted(bool runStarted)
    {
        this.runStarted = runStarted;
    }

    private void OnDestroy()
    {
        TutorialEvents.OnSetTimerStarted -= SetRunStarted;
    }
}