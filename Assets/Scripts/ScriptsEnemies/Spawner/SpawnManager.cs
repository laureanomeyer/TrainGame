using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    //se setean con el level spawn data
    float spawnInterval; 
    int maxEnemies;

    [Header("Levels")]

    [SerializeField] List<LevelSpawnsData> levelList = new();
    LevelSpawnsData currentlevelData;

    List<SpawnZone> activeZones = new();


    [Header("Coins")]
    [SerializeField] GameObject coin;

    private List<IWagon> trainList = new();

    List<EnemyData> spawnPool = new();

    Camera cam;

    float timer;
    int aliveEnemies;

    private void OnEnable()
    {
        GameEvents.OnEnemyDeath += EnemyDead;
    }

    private void OnDisable()
    {
        GameEvents.OnEnemyDeath -= EnemyDead;
    }


    public void Start()
    {
        trainList = RunManager.Instance.TrainCopyData.WagonList;
        cam = Camera.main;
        SetLevelData();
        BuildPool();
        //Debug.Log("hola si probando, nivel: " + GameManager.Instance.RunNumber + "level spawn: " + currentlevelData.name);
        TrySpawn();
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

    public void RegisterZone(SpawnZone zone)
    {
        if (!activeZones.Contains(zone))
        {
           // Debug.Log("zone register");
            activeZones.Add(zone);
        }
            
    }

    void TrySpawn()
    {
        if (activeZones.Count == 0) 
        {
            Debug.Log("lista vacia");
        }

        for (int i = 0; i < 10; i++)
        {
            SpawnZone zone = activeZones[Random.Range(0, activeZones.Count)];

            Vector3 point = zone.GetRandomPoint();

            if (IsOutsideCamera(point))
            {
                Spawn(point);
                return;
            }
        }
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
        GameObject enemyGO = Instantiate(currentlevelData.prefab, pos, Quaternion.identity);
        Enemy enemy = enemyGO.GetComponent<Enemy>();
        enemy.Initialize(enemyToSpawn);
        enemy.SetTargetList(trainList);

        aliveEnemies++;
    }

    void SetLevelData()
    {
        int index = GameManager.Instance.RunNumber;

        if (index > levelList.Count-1)
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

    void SpawCoin(Vector3 position, Vector3 goTo)
    {
        
    }

    void EnemyDead()
    {

        aliveEnemies--;
    }
}