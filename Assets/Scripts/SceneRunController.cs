using UnityEngine;

public class SceneRunController : MonoBehaviour
{
    [Header("Scene duration")]
    private float sceneDuration;

    private float currentTime;
    private bool runFinished;
    private bool runStarted = true;

    public float Progress => 1f - Mathf.Clamp01(currentTime / sceneDuration);

    private void Awake()
    {
        sceneDuration = GameManager.Instance.Session.SessionConfig.RunDurantion;
        currentTime = sceneDuration;
        runFinished = false;
        TutorialEvents.OnSetTimerStarted += SetRunStarted;
    }


    void Update()
    {
        if (runFinished) return;
        if (!GameManager.Instance.IsGameplayState) return;

        if (runStarted) currentTime -= Time.deltaTime;

        if (currentTime < 0)
        {
            runFinished = true;
            RunManager.Instance.OnRunFinished();
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
