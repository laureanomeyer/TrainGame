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

    [Header("Pool")]
    [SerializeField] private int defaultCapacity = 10;
    [SerializeField] private int maxSize = 30;

    public bool isAlive = true;

    [Header("Debug (solo lectura)")]
    [SerializeField] private int currentAmmo;
    [SerializeField] private bool isReloading;

    // Stats seteadas por el MotivationalBrain
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

    // ---------- Stats accesibles desde el brain (lectura y escritura) ----------

    public float Damage
    {
        get => damage;
        set => damage = Mathf.Max(0f, value);
    }

    /// <summary>Tiempo entre disparo y disparo (cadencia).</summary>
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
            // Si el cargador se achica, evita que sobren balas
            currentAmmo = Mathf.Min(currentAmmo, magazineSize);
        }
    }

    // Solo lectura, para UI
    public int CurrentAmmo => currentAmmo;
    public bool IsReloading => isReloading;

    /// <summary>
    /// Setea las stats y deja el cargador lleno.
    /// Se usa una sola vez al inicializar. Para cambios posteriores,
    /// usá las propiedades (Damage, FireCooldown, etc.).
    /// </summary>
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
        // Si se desactiva en medio de una recarga, la cancelamos
        isReloading = false;
        reloadTimer = 0f;
    }

    void Update()
    {
        if (!isAlive) return;
        if (!isInitialized) return;

        // Manejo de la recarga
        if (isReloading)
        {
            reloadTimer -= Time.deltaTime;

            if (reloadTimer <= 0f)
            {
                FinishReload();
            }

            return; // Mientras recarga, no dispara
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

        // Rotación base: apuntando al enemigo
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

        // Un solo cartucho por disparo, sin importar cuántos perdigones salgan
        currentAmmo--;

        isOnScreen = CameraView.IsInsideCamera(transform.position, Cam);
        AudioManager.Instance.PlayOnScreen("SFXTico&TacoShoot", isOnScreen);

        return true;
    }

    private void StartReload()
    {
        isReloading = true;
        reloadTimer = reloadTime;
        // Acá podés disparar un sonido o animación de recarga
    }

    private void FinishReload()
    {
        currentAmmo = magazineSize;
        isReloading = false;
        cooldownTimer = 0f;
    }

    // Opcional: llamalo si querés revivir/reactivar la torreta
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