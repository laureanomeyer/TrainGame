using UnityEngine;

public class NormalBullet : MonoBehaviour, IBullet
{
    [Header("Bullet velocity")]
    [SerializeField] float speed;

    [Header("Damage")]
    [SerializeField] int damage;

    [Header("Bullet duration")]
    [SerializeField] float duration;
    private float currentLife;

    private Rigidbody rb;
    private BoxCollider bc;
    public string id => "normal";

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        bc = GetComponent<BoxCollider>();

        currentLife = duration;
    }

    void Update()
    {
        Movement();
        TimeUntilDestroy();
    }

    public void ResetState()
    {

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
            DestroyBullet();
        }
    }

    public void DestroyBullet()
    {
        Destroy(this.gameObject);
    }

    public void OnDestroy()
    {

    }

}
