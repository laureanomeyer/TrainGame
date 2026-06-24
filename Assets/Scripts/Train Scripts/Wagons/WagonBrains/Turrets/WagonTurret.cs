using UnityEngine;
using UnityEngine.Pool;

public class WagonTurret : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform turretPivot;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform playerUsePoint;

    [Header("Weapon Data")]
    [SerializeField] private BulletTypeScriptable bulletType;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireCooldown = 0.03f;

    [Header("Pool")]
    [SerializeField] private int defaultCapacity = 15;
    [SerializeField] private int maxSize = 50;

    private IObjectPool<GameObject> bulletPool;
    private float cooldownTimer;
    private bool isOccupied;

    public bool IsOccupied => isOccupied;
    public Transform FirePoint => firePoint;
    public Transform PlayerUsePoint => playerUsePoint;

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

        WarmUp(defaultCapacity);
    }

    private void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
    }

    public void EnterTurret()
    {
        if (isOccupied)
            return;

        isOccupied = true;
    }

    public void ExitTurret()
    {
        isOccupied = false;
    }

    public void Aim(Vector3 direction)
    {
        if (turretPivot == null)
            return;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        turretPivot.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }

    public void TryShoot(Vector3 direction)
    {
        if (!isOccupied) return;
        if (cooldownTimer > 0f) return;
        if (firePoint == null) return;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        direction.Normalize();

        Aim(direction);

        GameObject bulletGO = bulletPool.Get();

        bulletGO.transform.SetPositionAndRotation(
            firePoint.position,
            Quaternion.LookRotation(direction, Vector3.up)
        );

        if (bulletGO.TryGetComponent(out IBullet bullet))
            bullet.ResetState(bulletType);

        cooldownTimer = fireCooldown;
    }

    private GameObject CreateBullet()
    {
        GameObject bulletGO = Instantiate(bulletPrefab);

        if (bulletGO.TryGetComponent(out IBullet bullet))
            bullet.BulletPool = bulletPool;

        bulletGO.SetActive(false);
        return bulletGO;
    }

    private void OnTakeBulletFromPool(GameObject bulletGO)
    {
        bulletGO.SetActive(true);
    }

    private void OnReturnBulletToPool(GameObject bulletGO)
    {
        if (bulletGO.TryGetComponent(out Rigidbody rb))
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
        GameObject[] bullets = new GameObject[count];

        for (int i = 0; i < count; i++)
            bullets[i] = bulletPool.Get();

        for (int i = 0; i < count; i++)
            bulletPool.Release(bullets[i]);
    }
}