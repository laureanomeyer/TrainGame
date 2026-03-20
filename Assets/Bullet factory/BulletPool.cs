using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Pool;

public class BulletPool : MonoBehaviour
{
    private BulletFactory factory;

    [SerializeField] private GameObject bullets;

    private IObjectPool<GameObject> bulletPool;

    private bool collectionCheck = true;

    private int defaultCapacity;
    private int maxCapacity = 1; //For evading MaxSizeError

    public int DefaultCapacity { set => defaultCapacity = value; }
    public int MaxCapacity { set => maxCapacity = value; }

    private void Start()
    {
        factory = GetComponent<BulletFactory>();
        bulletPool = new ObjectPool<GameObject>(CreateProjectile, OnGetFromPool, OnReleaseToPool, OnDestroyPoolObject, collectionCheck, defaultCapacity, maxCapacity);
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

    public void ShootObject(Vector3 position, Quaternion rotation, BulletTypeScriptable bulletType)
    {
        GameObject bullet = bulletPool.Get();
        bullet.GetComponent<IBullet>().ResetState(bulletType);

        if (bullet == null) return; //ThrowException (?) Sino, por qué tiraría null? No es posible sobrecargar los cohetes en el juego actual.

        bullet.transform.SetLocalPositionAndRotation(position, rotation);
    }
}
