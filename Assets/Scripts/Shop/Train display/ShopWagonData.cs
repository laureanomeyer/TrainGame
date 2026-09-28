using UnityEngine;

public class ShopWagonData : MonoBehaviour
{
    [SerializeField] public Transform tail;
    public IWagonID IDReference;
    public string CinematicKey;

    private Vector3 localFootprint;

    public Vector3 FootprintOffset => transform.rotation * localFootprint;

    private void Awake()
    {
        localFootprint = Quaternion.Inverse(transform.rotation) * (tail.position - transform.position);
    }

    public void SetID(IWagonID id)
    {
        IDReference = id;
    }
}