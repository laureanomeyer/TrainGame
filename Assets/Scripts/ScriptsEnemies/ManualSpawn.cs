using UnityEngine;

public class ManualSpawn : MonoBehaviour
{
    [SerializeField] private EnemyData enemyData;
    [SerializeField, Min(0)] private int quantity = 1;
    [SerializeField, Range(0f, 1f)] private float spawnAtProgress;

    private SpawnController spawnController;
    private SessionConfig sessionConfig;
    private bool hasSpawned;

    private void Start()
    {
        spawnController = ServiceLocator.Get<SpawnController>();
        sessionConfig = ServiceLocator.Get<SessionConfig>();

        if (sessionConfig == null)
        {
            Debug.LogError("ManualSpawn requires a SceneRunController in the scene.", this);
            enabled = false;
        }
    }

    private void Update()
    {
        if (hasSpawned || sessionConfig.RunDurantion < spawnAtProgress)
            return;

        hasSpawned = true;

        if (enemyData == null)
        {
            Debug.LogError("ManualSpawn requires EnemyData to be assigned.", this);
            return;
        }

        for (int i = 0; i < quantity; i++)
            spawnController.ManualSpawnEnemy(transform.position, enemyData);
    }
}
