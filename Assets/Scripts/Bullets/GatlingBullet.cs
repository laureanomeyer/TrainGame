using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(MeshFilter))]
public class GatlingBullet : MonoBehaviour, IBullet
{
    public string id => "Gatling";

    public float Damage { get; set; }
    public int Speed { get; set; }

    private bool destroyOnEnemy;
    public bool DestroyOnEnemy { get => destroyOnEnemy; }

    public IObjectPool<GameObject> BulletPool
    {
        set => bulletPool = value;
    }

    private IObjectPool<GameObject> bulletPool;

    private Rigidbody rb;
    private MeshFilter meshFilter;

    private BulletTypeScriptable currentType;
    private float currentLife;
    private bool isActive;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        meshFilter = GetComponent<MeshFilter>();
    }

    private void OnEnable()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    private void Update()
    {
        if (!isActive) return;

        currentLife -= Time.deltaTime;

        if (currentLife <= 0f)
        {
            Deactivate();
        }
    }

    private void FixedUpdate()
    {
        if (!isActive) return;

        Movement();
    }

    public void ResetState(BulletTypeScriptable type)
    {
        if (type == null) return;

        currentType = type;

        Damage = type.Damage;
        Speed = Mathf.RoundToInt(type.speed);
        currentLife = type.duration;

        if (meshFilter != null && type.bulletMesh != null)
        {
            meshFilter.mesh = type.bulletMesh;
        }

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        isActive = true;
    }

    public void Movement()
    {
        rb.linearVelocity = transform.forward * Speed;
    }

    public void Deactivate()
    {
        if (!isActive) return;

        isActive = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        bulletPool?.Release(gameObject);
    }
}