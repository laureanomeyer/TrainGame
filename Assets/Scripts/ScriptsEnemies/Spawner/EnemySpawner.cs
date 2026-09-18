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



    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, activationDistance);
    }


}