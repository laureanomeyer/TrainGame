using UnityEngine;
using System.Collections.Generic;



[CreateAssetMenu(menuName = "Levels/Spawns")]
public class LevelSpawnsData : ScriptableObject
{
    public List<SpawnEntry> spawneables;
    public GameObject prefab;

}

[System.Serializable]
public class SpawnEntry
{
    public EnemyData enemyData;
    public int quantity;
}