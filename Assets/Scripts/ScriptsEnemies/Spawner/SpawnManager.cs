using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] List<GameObject> enemyPrefabs;

    [SerializeField] float spawnInterval;
    [SerializeField] int maxEnemies;

    List<SpawnZone> activeZones = new();

    private List<IWagon> trainList = new();

    Camera cam;

    float timer;
    int aliveEnemies;

    public void Start()
    {
        trainList = RunManager.Instance.TrainCopyData.WagonList;
        cam = Camera.main;
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

    void Spawn(Vector3 pos)
    {
        GameObject enemy = Instantiate(enemyPrefabs[Random.Range(0, enemyPrefabs.Count)],pos,Quaternion.identity);
        enemy.GetComponent<Enemy>().SetTargetList(trainList);

        aliveEnemies++;
        Debug.Log("enemy spawn");
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