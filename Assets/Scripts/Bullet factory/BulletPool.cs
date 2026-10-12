using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Pool;

public class BulletPool : MonoBehaviour
{
    private BulletFactory factory;

    [SerializeField] private GameObject bullets;

    private IObjectPool<GameObject> bulletPool;

    private bool collectionCheck = true;

    [SerializeField] private int defaultCapacity = 10;
    [SerializeField] private int maxCapacity = 50;

    private void Awake()
    {
        ServiceLocator.Register(this);
        Debug.Log("BulletPool registered in ServiceLocator.");
    }

    public void InitializePool(int addDefaultCapacity = 0, int addMaxCapacity = 0)
    {
        defaultCapacity += addDefaultCapacity;
        maxCapacity += addMaxCapacity;

        factory = GetComponent<BulletFactory>();
        bulletPool = new ObjectPool<GameObject>(CreateProjectile, OnGetFromPool, OnReleaseToPool, OnDestroyPoolObject, collectionCheck, defaultCapacity, maxCapacity);
        WarmUp(defaultCapacity);
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister<BulletPool>();
        Debug.Log("BulletPool destroyed and unregistered from ServiceLocator.");
    }

    private GameObject CreateProjectile() //Functions as internal Awake()
    {
        GameObject projectile = factory.Create(bullets.GetComponent<IBullet>().id);
        projectile.GetComponent<IBullet>().BulletPool = bulletPool;
        return projectile;
    }

    private void OnGetFromPool(GameObject poolObject)
    {
        poolObject.gameObject.SetActive(true);
    }

    private void OnReleaseToPool(GameObject poolObject)
    {
        poolObject.gameObject.SetActive(false);
    }

    private void OnDestroyPoolObject(GameObject poolObject)
    {
        IBullet rocketToUnregister = poolObject.GetComponent<IBullet>();
        rocketToUnregister.Deactivate();
        Destroy(poolObject.gameObject);
    }

    private void WarmUp(int count)
    {
        var prewarm = new GameObject[count];
        for (int i = 0; i < count; i++) prewarm[i] = bulletPool.Get();
        for (int i = 0; i < count; ++i) bulletPool.Release(prewarm[i]);
    }

    public void ShootObject(Vector3 position, Quaternion rotation, BulletTypeScriptable bulletType)
    {
        GameObject bullet = bulletPool.Get();

        if (bullet == null) return;

        bullet.transform.SetLocalPositionAndRotation(position, rotation);

        bullet.transform.position = position;
        bullet.transform.rotation = rotation;

        bullet.GetComponent<IBullet>().ResetState(bulletType);
    }
}
