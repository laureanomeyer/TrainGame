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

    public WagonStore(GameObject prefab, string wagonName, float price)
    {
        this.prefab = prefab;
        this.wagonName = wagonName;
        this.price = price;
    }


}
