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
    [SerializeField] private float fireCooldown = 1f;   // Tiempo entre disparo y disparo
    [SerializeField] private float shootAngle = 35f;

    [Header("Detección")]
    [SerializeField] private LayerMask enemyLayer;      // Layer de los enemigos
    [SerializeField] private int maxDetected = 32;      // Tamaño del buffer

    [Header("Cargador")]
    [SerializeField] private int magazineSize = 10;     // Balas por cargador
    [SerializeField] private float reloadTime = 2f;     // Segundos de recarga

    [Header("Pool")]
    [SerializeField] private int defaultCapacity = 10;
    [SerializeField] private int maxSize = 30;

    [Header("Damage")]
    [SerializeField] private float damage = 1;
    private float currentDamage;

    public bool isAlive = true;

    private float cooldownTimer;
    private int currentAmmo;
    private bool isReloading;
    private float reloadTimer;

    private Collider[] hitsBuffer;

    public BulletPool bulletPool;

    Camera Cam => Camera.main;
    bool isOnScreen;

    // Para mostrar en UI si lo necesitás
    public int CurrentAmmo => currentAmmo;
    public int MagazineSize => magazineSize;
    public bool IsReloading => isReloading;

    void Awake()
    {
        hitsBuffer = new Collider[maxDetected];
    }

    void Start()
    {
        currentAmmo = magazineSize;
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

        // Solo se busca cuando realmente puede disparar
        Enemy target = FindTargetInCone();

        if (target != null && Shoot(target))
        {
            cooldownTimer = fireCooldown;

            if (currentAmmo <= 0)
            {
                StartReload();
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

        var statsRef = ServiceLocator.Get<StatSystem>();
        bulletType.Damage = damage * statsRef.GetStat(StatType.DamageMultiplier);
        bulletPool.ShootObject(firePoint.position, Quaternion.LookRotation(direction.normalized, Vector3.up), bulletType);

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

    private Enemy FindTargetInCone()
    {
        if (detectPoint == null) return null;

        int count = Physics.OverlapSphereNonAlloc(
            detectPoint.position,
            range,
            hitsBuffer,
            enemyLayer,
            QueryTriggerInteraction.Collide);

        Enemy bestTarget = null;
        float bestSqrDistance = Mathf.Infinity;

        for (int i = 0; i < count; i++)
        {
            Enemy enemy = hitsBuffer[i].GetComponentInParent<Enemy>();
            if (enemy == null) continue;

            Vector3 toEnemy = enemy.transform.position - detectPoint.position;
            toEnemy.y = 0f;

            float sqrDistance = toEnemy.sqrMagnitude;
            if (sqrDistance <= 0.0001f) continue;
            if (sqrDistance >= bestSqrDistance) continue;

            float angle = Vector3.Angle(detectPoint.forward, toEnemy);
            if (angle > shootAngle) continue;

            bestSqrDistance = sqrDistance;
            bestTarget = enemy;
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