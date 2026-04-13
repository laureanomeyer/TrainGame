using UnityEngine;

public class SceneRunController : MonoBehaviour
{
    [Header("Scene duration")]
    [SerializeField] private float sceneDuration;

    private float currentTime;

    private void Awake()
    {
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
