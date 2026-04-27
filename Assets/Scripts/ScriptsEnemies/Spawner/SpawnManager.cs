using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] List<GameObject> enemyPrefabs;

    [SerializeField] float spawnInterval;
    [SerializeField] int maxEnemies;
    [SerializeField] LevelSpawnsData levelData;

    List<SpawnZone> activeZones = new();

    private List<IWagon> trainList = new();

    List<EnemyData> spawnPool = new();

    Camera cam;

    float timer;
    int aliveEnemies;

    public void Start()
    {
        trainList = RunManager.Instance.TrainCopyData.WagonList;
        cam = Camera.main;
        BuildPool();
    }

    void Update()
    {

        timer += Time.deltaTime;

        if (timer >= spawnInterval && aliveEnemies < maxEnemies)
        {
            TrySpawn();
            timer = 0;
        }
    }

    public void RegisterZone(SpawnZone zone)
    {
        if (!activeZones.Contains(zone))
        {
            Debug.Log("zone register");
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

        foreach (var entry in levelData.spawneables)
        {
            for (int i = 0; i < entry.quantity; i++)
            {
                spawnPool.Add(entry.enemyData);
            }
        }
    }

    void Spawn(Vector3 pos)
    {
        EnemyData enemyToSpawn = spawnPool[Random.Range(0, spawnPool.Count)];
        GameObject enemyGO = Instantiate(levelData.prefab, pos, Quaternion.identity);
        Enemy enemy = enemyGO.GetComponent<Enemy>();
        enemy.Initialize(enemyToSpawn);
        enemy.SetTargetList(trainList);
    }

    bool IsOutsideCamera(Vector3 worldPos)
    {
        Vector3 vp = cam.WorldToViewportPoint(worldPos);

        return
            vp.x < 0 || vp.x > 1 ||
            vp.y < 0 || vp.y > 1 ||
            vp.z < 0;
    }
}