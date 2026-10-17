
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
        tr.emitting = true;
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
            // El objeto SÍ tiene la interfaz. Aquí puedes usarla directamente.
            reactive.OnDamage();
            if (destroyOnEnemy)
            {
                hasHit = true;
                Deactivate();
                return;
            }
        }

        if (enemyLayerDetection.value == (1 << other.gameObject.layer))
        {
            Enemy collisionEnemy = other.gameObject.GetComponent<Enemy>();
            if (collisionEnemy.HealthState == EnemyHealthState.Dead || collisionEnemy.HealthState == EnemyHealthState.Grace) return;
            bulletType.typeOfCollsion.BulletCollision(collisionEnemy, this);
            if (destroyOnEnemy) hasHit = true;
        }
    }

}
