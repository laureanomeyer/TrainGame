using UnityEngine;
using System.Collections.Generic;



[CreateAssetMenu(menuName = "Levels/Spawns")]
public class LevelSpawnsData : ScriptableObject
{
    public List<SpawnEntry> spawneables;
    public GameObject prefab;
    public int maxAliveEnemies;
    public int MaxHordeSpawn;
    public float spawnInterval;
    
}

[System.Serializable]
public class SpawnEntry
{
    public EnemyData enemyData;
    public int quantity;
}