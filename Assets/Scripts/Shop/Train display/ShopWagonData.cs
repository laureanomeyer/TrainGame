using UnityEngine;

public class ShopWagonData : MonoBehaviour
{
    [SerializeField] public Transform tail;
    public IWagonID IDReference;
    public string CinematicKey;

    public Vector3 FootprintOffset => tail.position - transform.position;

    public void SetID(IWagonID id)
    {
        IDReference = id;
    }
}