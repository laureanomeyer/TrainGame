using UnityEngine;
using UnityEngine.Pool;

public class WagonActiveTurret : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private BulletTypeScriptable bulletType;

    [Header("Ataque")]
    [SerializeField] private float fireCooldown = 0.15f;

    [Header("Pool")]
    [SerializeField] private int defaultCapacity = 15;
    [SerializeField] private int maxSize = 50;

    private float cooldownTimer;
    private IObjectPool<GameObject> bulletPool;

    private void Awake()
    {
        bulletPool = new ObjectPool<GameObject>(
            CreateBullet,
            OnTakeBulletFromPool,
            OnReturnBulletToPool,
            OnDestroyBullet,
            true,
            defaultCapacity,
            maxSize
        );
    }

    private void Update()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    public void TryShoot(Vector3 direction)
    {
        if (firePoint == null) return;
        if (cooldownTimer > 0f) return;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f) return;

        direction.Normalize();

        transform.forward = direction;
        Shoot(direction);

        cooldownTimer = fireCooldown;
    }

    private void Shoot(Vector3 direction)
    {
        GameObject bulletGO = bulletPool.Get();
        bulletGO.transform.position = firePoint.position;
        bulletGO.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

        IBullet bullet = bulletGO.GetComponent<IBullet>();
        if (bullet != null)
        {
            bullet.ResetState(bulletType);
        }
    }

    private GameObject CreateBullet()
    {
        GameObject bulletGO = Instantiate(bulletPrefab);

        IBullet bullet = bulletGO.GetComponent<IBullet>();
        if (bullet != null)
        {
            bullet.BulletPool = bulletPool;
        }

        bulletGO.SetActive(false);
        return bulletGO;
    }

    private void OnTakeBulletFromPool(GameObject bulletGO)
    {
        bulletGO.SetActive(true);
    }

    private void OnReturnBulletToPool(GameObject bulletGO)
    {
        bulletGO.SetActive(false);
    }

    private void OnDestroyBullet(GameObject bulletGO)
    {
        Destroy(bulletGO);
    }
}