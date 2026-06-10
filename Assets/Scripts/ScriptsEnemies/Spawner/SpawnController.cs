using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

public class SpawnController : MonoBehaviour
{
    //se setean con el level spawn data
    float spawnInterval;
    int maxEnemies;
    bool canSpawn = true;

    [Header("Levels")]

    [SerializeField] List<LevelSpawnsData> levelList = new();
    LevelSpawnsData currentlevelData;

    TrainRanges trainRanges = new TrainRanges();

    [SerializeField] SpawnZone spawnZone;

    [Header("Coins")]
    [SerializeField] GameObject coin;
    private Transform goldBox;

    [Header("Particle Systems")]
    [SerializeField] ParticleSystem enemyHitPS;

    private List<IWagon> trainList = new();

    List<EnemyData> spawnPool = new();

    Camera cam;

    float timer;
    int aliveEnemies;


    private void OnEnable()
    {
        GameEvents.OnEnemyDeath += EnemyDead;
        GameEvents.OnEnemyHit += EnemyHit;
        TutorialEvents.OnSpawnEnemy += SpawnSingleEnemy;
        TutorialEvents.OnStartSpawningEnemies += SetCanSpawn;

        canSpawn = !GameManager.Instance.IsTutorial;
    }

    private void OnDisable()
    {
        GameEvents.OnEnemyDeath -= EnemyDead;
        GameEvents.OnEnemyHit -= EnemyHit;
        TutorialEvents.OnSpawnEnemy -= SpawnSingleEnemy;
        TutorialEvents.OnStartSpawningEnemies -= SetCanSpawn;
    }


    public void Start()
    {
        trainList = RunManager.Instance.ActiveWagons;
        cam = Camera.main;
        SetLevelData();
        BuildPool();
        TrySpawn();
        goldBox = GameManager.Instance.Session.TrainData.GoldBoxPosition;

    }

    void Update()
    {

        timer += Time.deltaTime;

        if (timer >= spawnInterval && aliveEnemies < maxEnemies && spawnPool.Count > 0)
        {
            TrySpawn();
            timer = 0;
        }
    }


    void TrySpawn()
    {

        if (!canSpawn) return;
        (float positive, float negative) = trainRanges.SetRanges(50, Vector3.zero);
        Vector3 spawnPos = spawnZone.GetRandomPoint(positive, negative);

        for (int i = 0; i < currentlevelData.MaxHordeSpawn; i++)
        {
            if (IsOutsideCamera(spawnPos))
            {
                Spawn(spawnPos);
                spawnPos = spawnZone.GetRandomPoint(positive, negative);

            }
        }
    }

    void SpawnSingleEnemy(Vector3 pos, List<IWagon> targetList)
    {
        SpawnSingle(pos, targetList);
    }

    void BuildPool()
    {
        spawnPool.Clear();

        foreach (var entry in currentlevelData.spawneables)
        {
            for (int i = 0; i < entry.quantity; i++)
            {
                spawnPool.Add(entry.enemyData);
            }
        }
    }

    void Spawn(Vector3 pos)
    {
        if (spawnPool.Count == 0) return;

        int index = Random.Range(0, spawnPool.Count);

        EnemyData enemyToSpawn = spawnPool[index];
        GameObject enemyGO = ObjectPoolManager.SpawnObject(currentlevelData.prefab, pos, Quaternion.identity);
        Enemy enemy = enemyGO.GetComponent<Enemy>();
        enemy.Initialize(enemyToSpawn);
        enemy.SetTargetList(trainList);

        aliveEnemies++;
    }
    void SpawnSingle(Vector3 pos, List<IWagon> target)
    {
        if (spawnPool.Count == 0) return;

        int index = Random.Range(0, spawnPool.Count);

        EnemyData enemyToSpawn = spawnPool[index];
        GameObject enemyGO = ObjectPoolManager.SpawnObject(currentlevelData.prefab, pos, Quaternion.identity);
        Enemy enemy = enemyGO.GetComponent<Enemy>();
        enemy.Initialize(enemyToSpawn);
        enemy.SetTargetList(target);

        aliveEnemies++;
    }

    void SetLevelData()
    {
        int index = GameManager.Instance.Session.SessionConfig.CurrentLevel;

        if (index > levelList.Count - 1)
            currentlevelData = levelList.Last();
        else
            currentlevelData = levelList[index];

        maxEnemies = currentlevelData.maxAliveEnemies;
        spawnInterval = currentlevelData.spawnInterval;
    }

    bool IsOutsideCamera(Vector3 worldPos)
    {
        Vector3 vp = cam.WorldToViewportPoint(worldPos);

        return
            vp.x < 0 || vp.x > 1 ||
            vp.y < 0 || vp.y > 1 ||
            vp.z < 0;
    }

    void SpawCoin(Vector3 position, Transform goTo)
    {
        GameObject coinGO = ObjectPoolManager.SpawnObject(coin, position, Quaternion.identity);
        Coin coinScript = coinGO.GetComponent<Coin>();
        coinScript.SetTarget(goTo);
    }

    void EnemyDead(Vector3 position)
    {
        SpawCoin(position, goldBox);
        aliveEnemies--;
    }

    void SpawnParticles(Vector3 position)
    {
        ParticleSystem PS = Instantiate(enemyHitPS, position, Quaternion.identity);
    }

    void EnemyHit(Vector3 position)
    {
        SpawnParticles(position);
    }

    void SetCanSpawn(bool canSpawn)
    {
        this.canSpawn = canSpawn;
    }
}