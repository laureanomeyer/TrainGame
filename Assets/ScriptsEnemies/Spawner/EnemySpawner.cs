using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] Transform target;

    [SerializeField] GameObject enemyPrefab;
    [SerializeField] float spawnInterval = 2f;
    [SerializeField] int maxEnemies = 5;

    float timer;
    int currentEnemies;

    public Transform Target => target;

    private void Start()
    {
        Spawn();
    }

    void Update()
    {
        //timer += Time.deltaTime;

        //if (timer >= spawnInterval && currentEnemies < maxEnemies)
        //{
        //    Spawn();
        //    timer = 0f;
        //}
    }

    void Spawn()
    {
        GameObject enemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        enemy.GetComponent<Enemy>().SetTarget(target);
        currentEnemies++;
    }
}