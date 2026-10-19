using UnityEngine;

public abstract class EnemyMovementSO : ScriptableObject
{
    public float speed;

    [Header("Avoidance")]
    [SerializeField] private float avoidanceRadius = 5f;
    [SerializeField] private float avoidanceStrength = 3f;
    [SerializeField] private LayerMask enemyLayer;

    public abstract void Begin(Enemy enemy);
    public abstract void Knockback(Enemy enemy);

    public void Move(Enemy enemy)
    {
        if (enemy.HealthState == EnemyHealthState.Dead)
            return;

        MoveBehaviour(enemy);
    }

    protected abstract void MoveBehaviour(Enemy enemy);

    protected Vector3 GetAvoidanceOffset(Enemy enemy)
    {
        Collider[] nearby = Physics.OverlapSphere(
            enemy.rb.position,
            avoidanceRadius,
            enemyLayer
        );

        Vector3 separation = Vector3.zero;
        int neighbors = 0;

        foreach (Collider other in nearby)
        {
            Enemy otherEnemy = other.GetComponentInParent<Enemy>();

            if (otherEnemy == null || otherEnemy == enemy)
                continue;

            if (otherEnemy.HealthState == EnemyHealthState.Dead ||
                otherEnemy.HealthState == EnemyHealthState.Grace)
                continue;

            Vector3 away = enemy.rb.position - otherEnemy.rb.position;
            away.y = 0f;

            float distance = away.magnitude;

            if (distance <= 0.001f || distance >= avoidanceRadius)
                continue;

            separation += away.normalized * (1f - distance / avoidanceRadius);

            neighbors++;
        }

        if (neighbors == 0)
            return Vector3.zero;

        return separation.normalized * avoidanceStrength;
    }
}