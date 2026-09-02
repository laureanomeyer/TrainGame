using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    // Dictionary en vez de List: búsqueda O(1) en vez de O(n)
    private static Dictionary<string, PooledObjectInfo> ObjectPools = new();

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

    public static GameObject SpawnObject(GameObject objectToSpawn, Vector3 spawnPosition, Quaternion spawnRotation)
    {
        string key = objectToSpawn.name;

        if (!ObjectPools.TryGetValue(key, out PooledObjectInfo pool))
        {
            pool = new PooledObjectInfo() { LookupString = key };
            ObjectPools.Add(key, pool);
        }

        // Limpia referencias a objetos destruidos y busca uno reutilizable
        GameObject spawneableObj = null;
        while (pool.inactiveObjects.Count > 0)
        {
            GameObject candidate = pool.inactiveObjects[0];
            pool.inactiveObjects.RemoveAt(0);

            if (candidate != null) // false si fue Destroy()
            {
                spawneableObj = candidate;
                break;
            }
        }

        if (spawneableObj == null)
        {
            EnsureParentExists();
            spawneableObj = Object.Instantiate(objectToSpawn, spawnPosition, spawnRotation, poolParentTransform);
        }
        else
        {
            spawneableObj.transform.position = spawnPosition;
            spawneableObj.transform.rotation = spawnRotation;
            spawneableObj.SetActive(true);
        }

        return spawneableObj;
    }

    public static void ReturnObjectToPool(GameObject obj)
    {
        string goName = obj.name.Replace("(Clone)", string.Empty);

        if (!ObjectPools.TryGetValue(goName, out PooledObjectInfo pool))
        {
            Debug.LogWarning("Quiere liberar un objeto no pooleado: " + obj.name);
            Object.Destroy(obj); // evita que quede activo y perdido en la escena
        }
        else
        {
            obj.SetActive(false);
            pool.inactiveObjects.Add(obj);
        }
    }

    // Opcional: prewarm para precrear objetos antes de que se necesiten
    public static void PrewarmPool(GameObject objectToSpawn, int count)
    {
        string key = objectToSpawn.name;

        if (!ObjectPools.TryGetValue(key, out PooledObjectInfo pool))
        {
            pool = new PooledObjectInfo() { LookupString = key };
            ObjectPools.Add(key, pool);
        }

        EnsureParentExists();

        for (int i = 0; i < count; i++)
        {
            GameObject obj = Object.Instantiate(objectToSpawn, Vector3.zero, Quaternion.identity, poolParentTransform);
            obj.SetActive(false);
            pool.inactiveObjects.Add(obj);
        }
    }

    // Opcional: limpiar todo al cambiar de escena, si no usás DontDestroyOnLoad
    public static void ClearAllPools()
    {
        ObjectPools.Clear();
        poolParentTransform = null;
    }
}

public class PooledObjectInfo
{
    public string LookupString;
    public List<GameObject> inactiveObjects = new List<GameObject>();
}