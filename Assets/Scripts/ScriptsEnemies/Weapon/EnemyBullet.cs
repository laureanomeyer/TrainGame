using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
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
            Debug.Log("Hice daño" + damage);
            component.TakeDamage(damage);
            ObjectPoolManager.ReturnObjectToPool(gameObject);
        }
    }
}