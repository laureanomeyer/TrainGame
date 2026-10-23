using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "VisualsFactory",
    menuName = "Enemies/Visuals Factory"
)]
public class VisualsFactory : ScriptableObject
{
    [Serializable]
    public class VisualEntry
    {
        public EnemyVisualID id;
        public GameObject prefab;
    }

    [SerializeField] private List<VisualEntry> visuals;

    private Dictionary<EnemyVisualID, GameObject> visualLookup;

    private void InitializeLookup()
    {
        if (visualLookup != null)
            return;

        visualLookup = new Dictionary<EnemyVisualID, GameObject>();

        foreach (VisualEntry entry in visuals)
        {
            if (entry.prefab == null)
                continue;

            if (!visualLookup.TryAdd(entry.id, entry.prefab))
            {
                Debug.LogError(
                    $"Visual duplicado para el ID {entry.id}",
                    this
                );
            }
        }
    }

    public GameObject GetPrefab(EnemyVisualID id)
    {
        InitializeLookup();

        if (visualLookup.TryGetValue(id, out GameObject prefab))
            return prefab;

        Debug.LogError($"No existe un visual configurado para {id}", this);
        return null;
    }
        public GameObject GetVisual(EnemyVisualID id, Transform parent)
    {
        GameObject prefab = GetPrefab(id);

        if (prefab == null)
            return null;

        GameObject visual = ObjectPoolManager.SpawnObject(
            prefab,
            parent.position,
            parent.rotation
        );

        if (visual == null)
            return null;

        visual.transform.SetParent(parent, false);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one;

        return visual;
    }

    public void ReturnVisual(GameObject visual)
    {
        if (visual == null)
            return;

        ObjectPoolManager.ReturnObjectToPool(visual);
    }
}