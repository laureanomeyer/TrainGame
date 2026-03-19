using NUnit.Framework.Internal;
using UnityEngine;
using UnityEngine.Pool;

public class BulletScript : MonoBehaviour, IBullet
{
    public string id => "Normal";
    public IObjectPool<GameObject> BulletPool { set => bulletPool = value; }
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

    public void Initialize(BulletTypeScriptable type)
    {
        bulletType = type;

        
        currentLife = bulletType.duration;

        isActive = true;
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
        //Destroy(this.gameObject);
        if (!isActive) return;

        isActive = false;
        bulletPool.Release(gameObject);
    }

}
