using UnityEngine;

public class SceneRunController : MonoBehaviour
{
    [Header("Scene duration")]
    private float sceneDuration;

    private float currentTime;
    private bool runFinished;

    public float Progress => 1f - Mathf.Clamp01(currentTime / sceneDuration);

    private void Awake()
    {
        sceneDuration = GameManager.Instance.Session.SessionConfig.RunDurantion;
        currentTime = sceneDuration;
        runFinished = false;
    }

    void Update()
    {
        if (runFinished) return;

        currentTime -= Time.deltaTime;

        if (currentTime < 0)
        {
            runFinished = true;
            RunManager.Instance.OnRunFinished();
        }
    }
}
