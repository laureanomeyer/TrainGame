
using UnityEngine;
using UnityEngine.Pool;

public class BulletScript : MonoBehaviour, IBullet
{
    public string id => "Normal";
    public IObjectPool<GameObject> BulletPool { set => bulletPool = value; }
    public float Damage { get => damage; set => damage = value; }
    private float damage;

    public float Speed { get => speed; set => speed = value; }
    private float speed;

    private bool destroyOnEnemy;
    public bool DestroyOnEnemy { get => destroyOnEnemy; }

    private IObjectPool<GameObject> bulletPool;

    public BulletTypeScriptable bulletType;

    [SerializeField] private LayerMask enemyLayerDetection;
    [SerializeField] private GameObject hitEffectPrefab;

    private Rigidbody rb;
    private BoxCollider bc;
    private MeshFilter meshFilter;
    private Renderer render;
    private TrailRenderer tr;

    private bool isActive = true;
    private bool hasHit;
    private float currentLife;

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        rb = GetComponent<Rigidbody>();
        bc = GetComponent<BoxCollider>();
        render = GetComponent<Renderer>();
        tr = GetComponent<TrailRenderer>();
    }

    void Update()
    {
        if (isActive)
        {
            Movement();
            TimeUntilDestroy();
        }
    }

    public void ResetState(BulletTypeScriptable type)
    {
        bulletType = type;
        meshFilter.mesh = bulletType.bulletMesh;
        currentLife = bulletType.duration;
        Damage = bulletType.Damage;
        //Damage = bulletType.Damage * (stats.GetStat(StatType.DamageMultiplier));
        Speed = bulletType.speed;
        destroyOnEnemy = bulletType.destroyOnEnemy;
        render.material = bulletType.bulletMaterial;
        tr.material = bulletType.trailMaterial;
        tr.time = bulletType.trailDuration;
        tr.emitting = bulletType.haveTail;
        isActive = true;
        hasHit = false;
    }

    public void Movement()
    {
        rb.linearVelocity = transform.forward * speed;
    }

    private void TimeUntilDestroy()
    {
        currentLife -= Time.deltaTime;

        if (currentLife < 0)
        {
            Deactivate();
        }
    }

    public void Deactivate()
    {
        if (!isActive) return;

        tr.emitting = false;
        tr.Clear();
        isActive = false;
        speed = 0;
        rb.linearVelocity = transform.forward * speed;
        bulletPool.Release(gameObject);
    }

    private void GetImpactData(Collider other, out Vector3 point, out Vector3 normal)
    {
        Vector3 dir = transform.forward;

        // Distancia que la bala recorre por step de física + margen,
        // para asegurar que el origen del rayo quede fuera del collider
        float backDistance = speed * Time.fixedDeltaTime * 2f + 0.1f;

        Ray ray = new Ray(transform.position - dir * backDistance, dir);

        if (other.Raycast(ray, out RaycastHit hit, backDistance * 2f))
        {
            point = hit.point;
            normal = hit.normal;
        }
        else
        {
            // Fallback: si el raycast falla (bala ya muy adentro, MeshCollider, etc.)
            point = other.ClosestPoint(transform.position);
            normal = -dir;
        }
    }

    private void SpawnHitEffect(Collider other)
    {
        GetImpactData(other, out Vector3 point, out Vector3 normal);

        Vector3 spawnPos = point + normal * 0.01f;
        Quaternion rot = Quaternion.LookRotation(normal);

        Instantiate(hitEffectPrefab, spawnPos, rot); // luego lo cambias por tu pool
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;

        if (other.gameObject.CompareTag("deadWall"))
        {
            Deactivate();
            return;
        }

        if (other.gameObject.TryGetComponent<IReactiveObject>(out IReactiveObject reactive))
        {
            SpawnHitEffect(other);
            reactive.OnDamage();

            if (destroyOnEnemy)
            {
                hasHit = true;
                Deactivate();
                return;
            }
        }

        Enemy collisionEnemy = other.GetComponentInParent<Enemy>();

        if (collisionEnemy != null)
        {
            if (collisionEnemy.HealthState == EnemyHealthState.Dead ||
                collisionEnemy.HealthState == EnemyHealthState.Grace)
                return;

            SpawnHitEffect(other);
            bulletType.typeOfCollsion.BulletCollision(collisionEnemy, this);

            if (destroyOnEnemy)
            {
                hasHit = true;
                Deactivate();
            }
        }
    }

}
