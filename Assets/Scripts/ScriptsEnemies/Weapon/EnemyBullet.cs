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
        var components = other.GetComponents<MonoBehaviour>();

        foreach (var comp in components)
        {
            if (comp is IDamagable brain)
            {
                brain.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
        }
    }
}