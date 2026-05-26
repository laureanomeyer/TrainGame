using UnityEngine;

public class WagonStore : IWagonID
{
    private string wagonName;
    public string WagonName { get => wagonName; }

    private GameObject prefab;
    public GameObject Prefab { get => prefab; set => prefab = value; }

    public WagonStore(GameObject prefab, string wagonName)
    {
        this.prefab = prefab;
        this.wagonName = wagonName;
    }


}
