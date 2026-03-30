using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 10f;
    Vector3 direction;

    public void Init(Vector3 dir)
    {
        direction = dir.normalized;
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }
}