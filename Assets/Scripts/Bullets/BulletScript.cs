using NUnit.Framework.Internal;
using UnityEngine;
using UnityEngine.Pool;

public class BulletScript : MonoBehaviour, IBullet
{
    public string id => "Normal";
    public IObjectPool<GameObject> BulletPool { set => bulletPool = value; }
    public float Damage { get => damage; set => damage = value; }
    private float damage;

    public int Speed { get => speed; set => speed = value; }
    private int speed;

    private bool destroyOnEnemy;
    public bool DestroyOnEnemy { get => destroyOnEnemy; }

    private IObjectPool<GameObject> bulletPool;

    public BulletTypeScriptable bulletType;

    private Rigidbody rb;
    private BoxCollider bc;
    private MeshFilter meshFilter;
    private Renderer render;
    private TrailRenderer tr;

    private TrainData dataRef;
    private StatSystem stats;

    private bool isActive = true;
    private float currentLife;

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        rb = GetComponent<Rigidbody>();
        bc = GetComponent<BoxCollider>();
        render = GetComponent<Renderer>();
        tr = GetComponent<TrailRenderer>();

        stats = ServiceLocator.Get<StatSystem>();
        dataRef = ServiceLocator.Get<TrainData>();
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
        Damage = bulletType.Damage * (stats.GetStat(StatType.DamageMultiplier));
        Speed = speed;
        destroyOnEnemy = bulletType.destroyOnEnemy;
        render.material = bulletType.bulletMaterial;
        tr.material = bulletType.trailMaterial;
        tr.emitting = true;
        isActive = true;
    }

    public void Movement()
    {
        rb.linearVelocity = transform.forward * bulletType.speed;
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
        bulletPool.Release(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("deadWall"))
        {
            Deactivate();
            return;
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Enemy collisionEnemy = other.gameObject.GetComponent<Enemy>();

            bulletType.typeOfCollsion.BulletCollision(collisionEnemy, this);
        }
    }

}
