using UnityEngine;
using UnityEngine.Pool;

public class WagonFixedTurret : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform detectPoint;
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

    Camera Cam => Camera.main;
    bool isOnScreen;

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
        WarmUp(defaultCapacity);
    }

    void Update()
    {
        cooldownTimer -= Time.deltaTime;

        Enemy target = FindTargetInCone();

        if (target != null && cooldownTimer <= 0f)
        {

            Debug.Log("Shooting at target");
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
    private void WarmUp(int count)
    {
        var prewarm = new GameObject[count];
        for (int i = 0; i < count; i++) prewarm[i] = bulletPool.Get();
        for (int i = 0; i < count; ++i) bulletPool.Release(prewarm[i]);
    }
    private void Shoot(Enemy target)
    {
        if (target == null) return;
        if (firePoint == null) return;

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

        isOnScreen = CameraView.IsInsideCamera(transform.position, Cam);
        AudioManager.Instance.PlayOnScreen("SFXTico&TacoShoot", isOnScreen);

    }

    private Enemy FindTargetInCone()
    {
        if (detectPoint == null) return null;

        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        Enemy bestTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null) continue;

            Vector3 toEnemy = enemy.transform.position - detectPoint.position;
            toEnemy.y = 0f;

            float distance = toEnemy.magnitude;
            if (distance > range) continue;
            if (distance <= 0.01f) continue;

            float angle = Vector3.Angle(detectPoint.forward, toEnemy.normalized);
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
        if (detectPoint == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(detectPoint.position, range);

        Vector3 forward = detectPoint.forward;
        Quaternion leftRot = Quaternion.AngleAxis(-shootAngle, Vector3.up);
        Quaternion rightRot = Quaternion.AngleAxis(shootAngle, Vector3.up);

        Vector3 leftDir = leftRot * forward;
        Vector3 rightDir = rightRot * forward;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(detectPoint.position, detectPoint.position + forward * range);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(detectPoint.position, detectPoint.position + leftDir * range);
        Gizmos.DrawLine(detectPoint.position, detectPoint.position + rightDir * range);
    }
}