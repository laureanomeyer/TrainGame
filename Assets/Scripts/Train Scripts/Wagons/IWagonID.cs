using UnityEngine;

public interface IWagonID
{
    public string WagonName { get; }
    public GameObject Prefab { get; set; }
    public float Price { get; set; }
    public int Level { get; set; }
    public WagonLevelSetSO LevelSet { get; set; }
}