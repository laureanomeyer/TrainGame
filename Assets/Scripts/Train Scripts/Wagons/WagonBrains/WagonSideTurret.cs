using UnityEngine;
using UnityEngine.Pool;

public class WagonFixedTurret : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private BulletTypeScriptable bulletType;

    [Header("Ataque")]
    [SerializeField] private float range = 12f;
    [SerializeField] private float fireCooldown = 1f;
    [SerializeField] private float shootAngle = 35f;

    [Header("Pool")]
    [SerializeField] private int defaultCapacity = 10;
    [SerializeField] private int maxSize = 30;

    private float cooldownTimer;
    private IObjectPool<GameObject> bulletPool;

    void Awake()
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

    void Update()
    {
        cooldownTimer -= Time.deltaTime;

        Enemy target = FindTargetInCone();

        if (target != null && cooldownTimer <= 0f)
        {
            Shoot(target);
            cooldownTimer = fireCooldown;
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

    private void Shoot(Enemy target)
    {
        if (target == null) return;

        GameObject bulletGO = bulletPool.Get();
        bulletGO.transform.position = firePoint.position;

        Vector3 direction = target.transform.position - firePoint.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            direction = firePoint.forward;
        }

        bulletGO.transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);

        IBullet bullet = bulletGO.GetComponent<IBullet>();
        if (bullet != null)
        {
            bullet.ResetState(bulletType);
        }
    }

    private Enemy FindTargetInCone()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        Enemy bestTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null) continue;

            Vector3 toEnemy = enemy.transform.position - firePoint.position;
            toEnemy.y = 0f;

            float distance = toEnemy.magnitude;
            if (distance > range) continue;
            if (distance <= 0.01f) continue;

            float angle = Vector3.Angle(firePoint.forward, toEnemy.normalized);
            if (angle > shootAngle) continue;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                bestTarget = enemy;
            }
        }

        return bestTarget;
    }

    private void OnDrawGizmosSelected()
    {
        if (firePoint == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(firePoint.position, range);

        Vector3 forward = firePoint.forward;
        Quaternion leftRot = Quaternion.AngleAxis(-shootAngle, Vector3.up);
        Quaternion rightRot = Quaternion.AngleAxis(shootAngle, Vector3.up);

        Vector3 leftDir = leftRot * forward;
        Vector3 rightDir = rightRot * forward;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(firePoint.position, firePoint.position + forward * range);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(firePoint.position, firePoint.position + leftDir * range);
        Gizmos.DrawLine(firePoint.position, firePoint.position + rightDir * range);
    }
}