using UnityEngine;

public class MotivationalSideTurret : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform detectPoint;
    [SerializeField] private BulletTypeScriptable bulletType;

    [Header("Ataque")]
    [SerializeField] private float range = 12f;
    [SerializeField] private float shootAngle = 35f;

    [Header("Escopeta")]
    [Tooltip("Cantidad de perdigones por disparo")]
    [SerializeField, Min(1)] private int pelletCount = 5;
    [Tooltip("Ángulo total de dispersión en grados")]
    [SerializeField, Range(0f, 90f)] private float spreadAngle = 30f;

    public bool isAlive = true;

    [Header("Debug (solo lectura)")]
    [SerializeField] private int currentAmmo;
    [SerializeField] private bool isReloading;

    private float damage;
    private float fireCooldown;
    private int magazineSize;
    private float reloadTime;

    private bool isInitialized;
    private float cooldownTimer;
    private float reloadTimer;

    public BulletPool bulletPool;

    Camera Cam => Camera.main;
    bool isOnScreen;

    public float Damage
    {
        get => damage;
        set => damage = Mathf.Max(0f, value);
    }

    public float FireCooldown
    {
        get => fireCooldown;
        set => fireCooldown = Mathf.Max(0.01f, value);
    }

    public float ReloadTime
    {
        get => reloadTime;
        set => reloadTime = Mathf.Max(0f, value);
    }

    public int MagazineSize
    {
        get => magazineSize;
        set
        {
            magazineSize = Mathf.Max(1, value);
            currentAmmo = Mathf.Min(currentAmmo, magazineSize);
        }
    }
    public int CurrentAmmo => currentAmmo;
    public bool IsReloading => isReloading;

    public void Initialize(float damage, float fireCooldown, int magazineSize, float reloadTime)
    {
        Damage = damage;
        FireCooldown = fireCooldown;
        ReloadTime = reloadTime;
        this.magazineSize = Mathf.Max(1, magazineSize);

        currentAmmo = this.magazineSize;
        isReloading = false;
        reloadTimer = 0f;
        cooldownTimer = 0f;

        isInitialized = true;
    }

    void OnDisable()
    {
        isReloading = false;
        reloadTimer = 0f;
    }

    void Update()
    {
        if (!isAlive) return;
        if (!isInitialized) return;

        if (isReloading)
        {
            reloadTimer -= Time.deltaTime;

            if (reloadTimer <= 0f)
            {
                FinishReload();
            }

            return;
        }

        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer > 0f) return;

        Enemy target = FindTargetInCone();

        if (target != null)
        {
            if (Shoot(target))
            {
                cooldownTimer = fireCooldown;

                if (currentAmmo <= 0)
                {
                    StartReload();
                }
            }
        }
    }

    private bool Shoot(Enemy target)
    {
        if (!isAlive) return false;
        if (target == null) return false;
        if (firePoint == null) return false;
        if (currentAmmo <= 0) return false;

        Vector3 direction = target.transform.position - firePoint.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            direction = firePoint.forward;
        }

        Quaternion baseRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);

        bulletType.Damage = damage;

        float angleStep = pelletCount > 1 ? spreadAngle / (pelletCount - 1) : 0f;
        float startAngle = -spreadAngle / 2f;

        for (int i = 0; i < pelletCount; i++)
        {
            float currentAngle = startAngle + angleStep * i;

            Quaternion spreadRotation = Quaternion.AngleAxis(currentAngle, Vector3.up);
            Quaternion finalRotation = spreadRotation * baseRotation;

            bulletPool.ShootObject(firePoint.position, finalRotation, bulletType);
        }

        currentAmmo--;

        isOnScreen = CameraView.IsInsideCamera(transform.position, Cam);
        AudioManager.Instance.PlayOnScreen("SFXTico&TacoShoot", isOnScreen);

        return true;
    }

    private void StartReload()
    {
        isReloading = true;
        reloadTimer = reloadTime;
    }

    private void FinishReload()
    {
        currentAmmo = magazineSize;
        isReloading = false;
        cooldownTimer = 0f;
    }

    public void Revive()
    {
        isAlive = true;
        currentAmmo = magazineSize;
        isReloading = false;
        cooldownTimer = 0f;
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
        if (!isAlive) return;
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