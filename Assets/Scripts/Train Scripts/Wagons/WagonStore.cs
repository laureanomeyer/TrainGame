using UnityEngine;

public class WagonStore : IWagonID
{
    private GameObject prefab;
    public GameObject Prefab { get => prefab; set => prefab = value; }

    public WagonStore(GameObject prefab)
    {
        this.prefab = prefab;
    }


}
