using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{

    [SerializeField] List<GameObject> enemyPrefab;
    [SerializeField] float spawnInterval = 5f;
    [SerializeField] int maxEnemies = 10;

    float timer;
    int currentEnemies;

    private List<IWagon> trainList = new();

    public float activationDistance = 60f;

    private void Awake()
    {
        trainList = RunManager.Instance.ActiveWagons;
    }

    void Update()
    {
        if (trainList == null || trainList.Count == 0) return;

        float distance = Vector3.Distance(transform.position, trainList[0].Transform.position);

        if (distance > activationDistance)
        {
            timer += Time.deltaTime;

            if (timer >= spawnInterval && currentEnemies < maxEnemies)
            {
                Spawn();
                timer = 0f;
            }
        } 
             


    }

    void Spawn()
    {
        GameObject enemy = Instantiate(enemyPrefab[Random.Range(0, enemyPrefab.Count)], transform.position, Quaternion.identity);
        enemy.GetComponent<Enemy>().SetTargetList(trainList);
        currentEnemies++;

    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, activationDistance);
    }


}