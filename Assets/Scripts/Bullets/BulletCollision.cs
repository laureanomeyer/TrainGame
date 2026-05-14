using Unity.VisualScripting;
using UnityEngine;

public class BulletCollision : MonoBehaviour
{
    private IBullet bullet;

    private void Awake()
    {
        bullet = GetComponent<IBullet>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("deadWall"))
        {
            bullet?.Deactivate();
            return;
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Enemy collisionEnemy = other.gameObject.GetComponent<Enemy>();
            collisionEnemy.TakeDamage(bullet.Damage);

            if (bullet.DestroyOnEnemy)
            {
                bullet?.Deactivate();
                return;
            } 
        }
    }
}