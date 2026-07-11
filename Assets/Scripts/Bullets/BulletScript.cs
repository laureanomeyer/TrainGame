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

    private bool isActive = true;
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
        dataRef = GameManager.Instance.Session._TrainData;

        bulletType = type;
        meshFilter.mesh = bulletType.bulletMesh;
        currentLife = bulletType.duration;
        Damage = bulletType.Damage * (GameManager.Instance.Session._StatSystem.GetStat(StatType.DamageMultiplier));
        Speed = speed;
        destroyOnEnemy = bulletType.destroyOnEnemy;
        render.material = bulletType.bulletMaterial;
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

        tr.emitting = true;
        tr.Clear();
        isActive = false;
        bulletPool.Release(gameObject);
    }

}
