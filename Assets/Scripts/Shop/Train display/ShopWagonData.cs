using UnityEngine;

public class ShopWagonData : MonoBehaviour
{
    [SerializeField] public Transform tail;
    public IWagonID IDReference;
    public string CinematicKey;

    private Vector3 localFootprint;
    private Renderer[] renderers;

    private Transform[] allTransforms;
    private int currentLayer = -1;

    public Vector3 LocalFootprint => localFootprint;
    public Vector3 FootprintOffset => transform.rotation * localFootprint;

    private void Awake()
    {
        localFootprint = Quaternion.Inverse(transform.rotation) * (tail.position - transform.position);
        renderers = GetComponentsInChildren<Renderer>(true);
        allTransforms = GetComponentsInChildren<Transform>(true);
    }

    public void SetID(IWagonID id)
    {
        IDReference = id;
    }

    #region Layers

    public void SetLayer(int layer)
    {
        if (layer < 0 || layer == currentLayer) return;
        currentLayer = layer;

        for (int i = 0; i < allTransforms.Length; i++)
        {
            if (allTransforms[i] != null)
                allTransforms[i].gameObject.layer = layer;
        }
    }

    #endregion

    #region Reordering

    public Bounds GetWorldBounds()
    {
        Bounds b = new(transform.position, Vector3.zero);
        bool init = false;

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];
            if (r == null || r is ParticleSystemRenderer) continue;

            if (!init) { b = r.bounds; init = true; }
            else b.Encapsulate(r.bounds);
        }

        return b;
    }

    #endregion
}