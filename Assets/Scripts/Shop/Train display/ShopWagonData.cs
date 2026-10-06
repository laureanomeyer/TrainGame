using UnityEngine;

public class ShopWagonData : MonoBehaviour
{
    [SerializeField] public Transform tail;
    public IWagonID IDReference;
    public string CinematicKey;

    private Vector3 localFootprint;
    private Renderer[] renderers;

    public Vector3 LocalFootprint => localFootprint;
    public Vector3 FootprintOffset => transform.rotation * localFootprint;

    private void Awake()
    {
        localFootprint = Quaternion.Inverse(transform.rotation) * (tail.position - transform.position);
        renderers = GetComponentsInChildren<Renderer>(true);
    }

    public void SetID(IWagonID id)
    {
        IDReference = id;
    }
    #region Reordering
    public Bounds GetWorldBounds()
    {
        Bounds b = new(transform.position, Vector3.zero);
        bool init = false;

        foreach (var r in renderers)
        {
            if (r == null || r is ParticleSystemRenderer) continue;

            if (!init) { b = r.bounds; init = true; }
            else b.Encapsulate(r.bounds);
        }

        return b;
    }
    #endregion
}