using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Movement/Normal")]
public class EnemyMovementNormal : EnemyMovementSO
{
    public override void Move(Enemy enemy)
    {
        Transform train = enemy.Target;

        if (train == null) return;

        Vector3 pos = enemy.rb.position;

        float minZ = enemy.Limits.Item1;
        float maxZ = enemy.Limits.Item2;

        bool insideLane = pos.z <= minZ && pos.z >= maxZ;

        // =========================
        // FUERA DEL CARRIL
        // =========================
        if (!insideLane)
        {
            Vector3 dir = (train.position - pos).normalized;

            enemy.rb.MovePosition(pos + dir * enemy.Speed * Time.deltaTime);
        }
        // =========================
        // DENTRO DEL CARRIL
        // =========================
        else
        {
            float targetX = train.position.x;

            float distanceToX = Mathf.Abs(pos.x - targetX);

            float stopDistance = 5f;

            if (distanceToX <= stopDistance) return;

            Vector3 lateralDir = pos.x < targetX
                ? Vector3.right
                : Vector3.left;

            Vector3 nextPos = pos + lateralDir * enemy.Speed * Time.deltaTime;

            enemy.rb.MovePosition(nextPos);
        }
    }
    public override void Knockback(Enemy enemy)
    {
        enemy.rb.AddForce(enemy.rb.transform.forward * 10, ForceMode.Impulse);
    }

}