using UnityEngine;
using UnityEngine.Pool;

public class GatlingWagonBrain : WagonBrain
{
    [Header("Turret")]
    [SerializeField] private Transform turretPivot;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform playerUsePoint;

    [Header("Attack")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private BulletTypeScriptable bulletType;
    [SerializeField] private float fireCooldown = 0.03f;

    [Header("Pool")]
    [SerializeField] private int defaultCapacity = 15;
    [SerializeField] private int maxSize = 50;

    private float cooldownTimer;
    private IObjectPool<GameObject> bulletPool;

    private bool isOccupied;
    private Transform currentPlayer;

    public bool IsOccupied => isOccupied;
    public Transform PlayerUsePoint => playerUsePoint;
    public Transform FirePoint => firePoint;

    public override void Start()
    {
        base.Start();

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

    public void Interact(Transform player)
    {
        if (isOccupied)
        {
            ExitTurret();
            return;
        }

        isOccupied = true;
        currentPlayer = player;

        // Acá después ponés al player en playerUsePoint
        // y bloqueás su movimiento desde PlayerInteractions.
    }

    public void ExitTurret()
    {
        isOccupied = false;
        currentPlayer = null;
    }

    public void TryShoot(Vector3 direction)
    {
        if (!isOccupied) return;
        if (firePoint == null) return;
        if (bulletPool == null) return;
        if (cooldownTimer > 0f) return;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        direction.Normalize();

        if (turretPivot != null)
            turretPivot.rotation = Quaternion.LookRotation(direction, Vector3.up);

        Shoot(direction);
        cooldownTimer = fireCooldown;
    }

    private void Shoot(Vector3 direction)
    {
        GameObject bulletGO = bulletPool.Get();

        bulletGO.transform.SetPositionAndRotation(
            firePoint.position,
            Quaternion.LookRotation(direction, Vector3.up)
        );

        if (bulletGO.TryGetComponent(out IBullet bullet))
            bullet.ResetState(bulletType);
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