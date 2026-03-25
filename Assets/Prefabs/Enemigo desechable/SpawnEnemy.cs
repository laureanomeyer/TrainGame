using System;
using UnityEngine;
public class EnemySpawner : MonoBehaviour
{
    public GameObject train;

    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
    public float spawnInterval = 2f;
    public int maxEnemies = 5;

    [Header("Distance Settings")]
    public float activationDistance = 20f;

    float timer;
    int currentEnemies;

    private void Start()
    {
        train = GameObject.FindGameObjectWithTag("Train");
    }

    void Update()
    {
        if (train == null) return;

        float distance = Vector3.Distance(transform.position, train.transform.position);

        Debug.Log(distance);
        if (distance <= activationDistance)
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
        Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        currentEnemies++;
    }
}