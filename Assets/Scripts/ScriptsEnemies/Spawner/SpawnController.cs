using UnityEngine;

public class SpawnController : MonoBehaviour
{
    [Header("Coins")]
    [SerializeField] private GameObject coin;
    private Transform goldBox;

    [Header("Coal")]
    [SerializeField] private GameObject coal;
    private Transform coalBox;

    [Header("Particle Systems")]
    [SerializeField] private ParticleSystem enemyHitPS;

    private TrainData trainDataRef;

    private void OnEnable()
    {
        ServiceLocator.Register(this);
        EventBus.Subscribe<OnEnemyDeathEvent>(EnemyDead);
        EventBus.Subscribe<OnEnemyHitEvent>(EnemyHit);
        trainDataRef = ServiceLocator.Get<TrainData>();
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<OnEnemyDeathEvent>(EnemyDead);
        EventBus.Unsubscribe<OnEnemyHitEvent>(EnemyHit);
        ServiceLocator.Unregister<SpawnController>();
    }

    private void Start()
    {
        goldBox = trainDataRef.GoldBoxPosition;
        coalBox = trainDataRef.CoalBoxPosition;
    }

    public Enemy SpawnEnemy(GameObject prefab, EnemyData data, Vector3 position, Quaternion rotation)
    {
        if (prefab == null || data == null)
        {
            Debug.LogError("Cannot spawn an enemy without a prefab and EnemyData.", this);
            return null;
        }

        GameObject enemyGO = ObjectPoolManager.SpawnObject(prefab, position, rotation);
        if (enemyGO == null)
        {
            Debug.LogError($"Failed to spawn enemy prefab '{prefab.name}'.", this);
            return null;
        }

        if (!enemyGO.TryGetComponent(out Enemy enemy))
        {
            Debug.LogError($"Enemy prefab '{prefab.name}' does not contain an Enemy component.", enemyGO);
            ObjectPoolManager.ReturnObjectToPool(enemyGO);
            return null;
        }

        enemy.Initialize(data);
        return enemy;
    }

    private void SpawnCoin(Vector3 position, Transform goTo)
    {
        GameObject coinGO = ObjectPoolManager.SpawnObject(coin, position, Quaternion.identity);
        Coin coinScript = coinGO.GetComponent<Coin>();
        coinScript.SetTarget(goTo);
        
    }

    private void SpawnCoal(Vector3 position, Transform goTo)
    {
        if (goTo == null) Debug.Log("coal box nulla");
        GameObject coalGO = ObjectPoolManager.SpawnObject(coal, position, Quaternion.identity);
        Coal coalScript = coalGO.GetComponent<Coal>();
        coalScript.SetTarget(goTo);
    }

    private void EnemyDead(OnEnemyDeathEvent enemyDeathEvent)
    {
        if (enemyDeathEvent.DropType == DropType.Gold)
            SpawnCoin(enemyDeathEvent.Position, goldBox);
        else if (enemyDeathEvent.DropType == DropType.Coal)
            SpawnCoal(enemyDeathEvent.Position, coalBox);
    }

    private void SpawnParticles(Vector3 position)
    {
        Instantiate(enemyHitPS, position, Quaternion.identity);
    }

    private void EnemyHit(OnEnemyHitEvent enemyHitEvent)
    {
        SpawnParticles(enemyHitEvent.Position);
    }
}