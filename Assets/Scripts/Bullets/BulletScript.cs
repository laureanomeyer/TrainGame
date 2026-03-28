using NUnit.Framework.Internal;
using UnityEngine;
using UnityEngine.Pool;

public class BulletScript : MonoBehaviour, IBullet
{
    public string id => "Normal";
    public IObjectPool<GameObject> BulletPool { set => bulletPool = value; }
    public int Damage { get => damage; set => damage = value; }
    private int damage;

    public int Speed { get => speed; set => speed = value; }
    private int speed;

    private IObjectPool<GameObject> bulletPool;

    public BulletTypeScriptable bulletType;

    private Rigidbody rb;
    private BoxCollider bc;
    private MeshFilter meshFilter;

    private bool isActive = true;
    private float currentLife;

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        rb = GetComponent<Rigidbody>();
        bc = GetComponent<BoxCollider>();
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
        Damage = bulletType.damage;
        Speed = speed;
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

        isActive = false;
        bulletPool.Release(gameObject);
    }

}
