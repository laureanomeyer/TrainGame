using UnityEngine;
using UnityEngine.Pool;

public class GatlingWagonBrain : WagonBrain
{
    [Header("Referencias Gatling")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private BulletTypeScriptable bulletType;

    [Header("Ataque")]
    [SerializeField] private float fireCooldown = 0.03f;

    [Header("Pool")]
    [SerializeField] private int defaultCapacity = 15;
    [SerializeField] private int maxSize = 50;

    private float cooldownTimer;
    private IObjectPool<GameObject> bulletPool;

    public Transform FirePoint => firePoint;

    public override void Start()
    {
        base.Start();
        SetUpWagonHP();

        bulletPool = new ObjectPool<GameObject>(
            CreateBullet,
            OnTakeBulletFromPool,
            OnReturnBulletToPool,
            OnDestroyBullet,
            true,
            defaultCapacity,
            maxSize
        );

        WarmUp(defaultCapacity);
    }

    public new void Update()
    {
        base.Update();

        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    public void TryShoot(Vector3 direction)
    {
        if (firePoint == null) return;
        if (bulletPrefab == null) return;
        if (bulletType == null) return;
        if (bulletPool == null) return;
        if (cooldownTimer > 0f) return;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f) return;

        direction.Normalize();

        Shoot(direction);
        cooldownTimer = fireCooldown;
    }

    private void Shoot(Vector3 direction)
    {
        GameObject bulletGO = bulletPool.Get();

        bulletGO.transform.position = firePoint.position;
        bulletGO.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

        if (bulletGO.TryGetComponent(out IBullet bullet))
        {
            bullet.ResetState(bulletType);
        }
    }

    private GameObject CreateBullet()
    {
        GameObject bulletGO = Instantiate(bulletPrefab);

        if (bulletGO.TryGetComponent(out IBullet bullet))
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
        Rigidbody rb = bulletGO.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        bulletGO.SetActive(false);
    }

    private void OnDestroyBullet(GameObject bulletGO)
    {
        Destroy(bulletGO);
    }

    private void WarmUp(int count)
    {
        GameObject[] prewarm = new GameObject[count];

        for (int i = 0; i < count; i++)
        {
            prewarm[i] = bulletPool.Get();
        }

        for (int i = 0; i < count; i++)
        {
            bulletPool.Release(prewarm[i]);
        }
    }
}