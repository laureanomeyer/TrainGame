using UnityEngine;

public class SceneRunController : MonoBehaviour
{
    [Header("Scene duration")]
    private float sceneDuration;

    private float currentTime;

    public float Progress => 1f - Mathf.Clamp01(currentTime / sceneDuration);

    private void Awake()
    {
        sceneDuration = GameManager.Instance.runduration;
        currentTime = sceneDuration;
    }

    void Update()
    {
        currentTime -= Time.deltaTime;

        if (currentTime < 0)
        {
            GameManager.Instance.GoToStore();
        }
    }
}
