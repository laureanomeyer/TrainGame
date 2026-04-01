using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{

    [SerializeField] GameObject enemyPrefab;
    [SerializeField] float spawnInterval = 5f;
    //[SerializeField] int maxEnemies = 5;

    float timer;
    int currentEnemies;

    private List<IWagon> trainList = new();

    private void Start()
    {
        trainList = GameManager.Instance.TrainData.WagonList;
        Spawn();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            Spawn();
            timer = 0f;
            spawnInterval += 1;
        }
    }

    void Spawn()
    {
        GameObject enemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        enemy.GetComponent<Enemy>().SetTargetList(trainList);
        currentEnemies++;
    }
}