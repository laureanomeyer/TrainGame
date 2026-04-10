using UnityEngine;

public class WagonStore : IWagonID
{
    private GameObject prefab;
    public GameObject Prefab => prefab;

    public WagonStore(GameObject prefab)
    {
        this.prefab = prefab;
    }


}
