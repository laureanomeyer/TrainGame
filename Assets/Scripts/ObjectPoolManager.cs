using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    private static Dictionary<int, PooledObjectInfo> ObjectPools = new();
    private static Dictionary<GameObject, int> PoolIdsByObject = new();

    // Contenedor raíz para no ensuciar la jerarquía de la escena
    private static Transform poolParentTransform;

    private static void EnsureParentExists()
    {
        if (poolParentTransform == null)
        {
            GameObject parentObj = new GameObject("Pooled Objects");
            poolParentTransform = parentObj.transform;
        }
    }

    private static PooledObjectInfo GetOrCreatePool(int id)
    {
        if (!ObjectPools.TryGetValue(id, out PooledObjectInfo pool))
        {
            pool = new PooledObjectInfo(id);
            ObjectPools.Add(id, pool);
        }

        return pool;
    }

    public static GameObject SpawnObject(GameObject objectToSpawn, Vector3 spawnPosition, Quaternion spawnRotation)
    {
        if (objectToSpawn == null) return null;

        int id = objectToSpawn.GetInstanceID();
        PooledObjectInfo pool = GetOrCreatePool(id);

        GameObject spawnableObject = null;
        while (pool.InactiveObjects.Count > 0)
        {
            GameObject candidate = pool.InactiveObjects.Dequeue();
            pool.InactiveSet.Remove(candidate);

            if (candidate != null)
            {
                spawnableObject = candidate;
                break;
            }

            PoolIdsByObject.Remove(candidate);
        }

        if (spawnableObject == null)
        {
            EnsureParentExists();
            spawnableObject = Object.Instantiate(objectToSpawn, spawnPosition, spawnRotation, poolParentTransform);
            PoolIdsByObject.Add(spawnableObject, id);
        }
        else
        {
            spawnableObject.transform.SetPositionAndRotation(spawnPosition, spawnRotation);
            spawnableObject.SetActive(true);
        }

        return spawnableObject;
    }

    public static void ReturnObjectToPool(GameObject obj)
    {
        if (obj == null) return;

        if (!PoolIdsByObject.TryGetValue(obj, out int id) || !ObjectPools.TryGetValue(id, out PooledObjectInfo pool))
        {
            Debug.LogWarning("Quiere liberar un objeto no pooleado: " + obj.name);
            Object.Destroy(obj);
            return;
        }

        if (!pool.InactiveSet.Add(obj)) return;

        obj.SetActive(false);
        pool.InactiveObjects.Enqueue(obj);
    }

    public static void PrewarmPool(GameObject objectToSpawn, int count)
    {
        if (objectToSpawn == null || count <= 0) return;

        int id = objectToSpawn.GetInstanceID();
        PooledObjectInfo pool = GetOrCreatePool(id);

        EnsureParentExists();

        for (int i = 0; i < count; i++)
        {
            GameObject obj = Object.Instantiate(objectToSpawn, Vector3.zero, Quaternion.identity, poolParentTransform);
            obj.SetActive(false);
            PoolIdsByObject.Add(obj, id);
            pool.InactiveObjects.Enqueue(obj);
            pool.InactiveSet.Add(obj);
        }
    }

    public static void ClearAllPools()
    {
        ObjectPools.Clear();
        PoolIdsByObject.Clear();
        poolParentTransform = null;
    }
}

public class PooledObjectInfo
{
    public int Id { get; }
    public Queue<GameObject> InactiveObjects { get; } = new();
    internal HashSet<GameObject> InactiveSet { get; } = new();

    public PooledObjectInfo(int id)
    {
        Id = id;
    }
}