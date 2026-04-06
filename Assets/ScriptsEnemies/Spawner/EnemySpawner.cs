using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{

    [SerializeField] GameObject enemyPrefab;
    [SerializeField] float spawnInterval = 5f;
    [SerializeField] int maxEnemies = 10;

    float timer;
    int currentEnemies;

    private List<IWagon> trainList = new();

    public float activationDistance = 20f;

    private void Start()
    {
        trainList = RunManager.Instance.TrainData.WagonList;
        Spawn();
    }

    void Update()
    {

        float distance = Vector3.Distance(transform.position, trainList[0].Transform.position);

        if (distance >= activationDistance)
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
        GameObject enemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        enemy.GetComponent<Enemy>().SetTargetList(trainList);
        currentEnemies++;
        Debug.Log("Cant enemies: " + currentEnemies);
    }
}