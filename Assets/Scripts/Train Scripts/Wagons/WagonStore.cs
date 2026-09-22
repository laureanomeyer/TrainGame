using System;
using UnityEngine;

[Serializable]
public class WagonStore : IWagonID
{
    private string wagonName;
    public string WagonName { get => wagonName; }

    private GameObject prefab;
    public GameObject Prefab { get => prefab; set => prefab = value; }

    private float price;
    public float Price { get => price; set => price = value; }

    private int level;
    public int Level { get => level; set => level = value; }

    private WagonLevelSetSO levelSet;
    public WagonLevelSetSO LevelSet { get => levelSet; set => levelSet = value; }

    public WagonStore(GameObject prefab, string wagonName, float price, WagonLevelSetSO levelSet = null, int level = 1)
    {
        this.prefab = prefab;
        this.wagonName = wagonName;
        this.price = price;
        this.levelSet = levelSet;
        this.level = level;
    }
}