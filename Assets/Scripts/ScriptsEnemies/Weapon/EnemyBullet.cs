using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [SerializeField] private TrailRenderer tr;
    public float speed = 10f;
    Vector3 direction;
    float damage;

    public void Init(Vector3 dir, float damage)
    {
        direction = dir.normalized;
        this.damage = damage;
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        IDamagable component = other.GetComponent<IDamagable>();
        if (component != null) 
        {
            component.TakeDamage(damage);
            tr.emitting = false;
            tr.Clear();
            tr.emitting = true;
            ObjectPoolManager.ReturnObjectToPool(gameObject);
        }
    }
}