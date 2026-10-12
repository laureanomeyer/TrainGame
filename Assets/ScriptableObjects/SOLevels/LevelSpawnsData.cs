using UnityEngine;
using System.Collections.Generic;



[CreateAssetMenu(menuName = "Levels/Spawns")]
public class LevelSpawnsData : ScriptableObject
{
    public List<SpawnEntry> spawneables;
    public List<SpawnEntryByTime> spawneablesByTime;
    public GameObject prefab;
    public int maxAliveEnemies;
    public float spawnInterval;
    public int MaxHordeSpawn;
}

[System.Serializable]
public class SpawnEntry
{
    public EnemyData enemyData;
    public int weight;
}
[System.Serializable]
public class SpawnEntryByTime
{
    public EnemyData enemyData;
    public bool onlyOnce;
    public float spawnInterval;
}