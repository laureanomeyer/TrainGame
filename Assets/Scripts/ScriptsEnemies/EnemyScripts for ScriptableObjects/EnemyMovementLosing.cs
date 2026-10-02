using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Movement/Losing")]
public class EnemyMovementLosing : EnemyMovementSO
{
    [Header("Losing settings")]
    [SerializeField] float timeBeforeLosing = 5f;
    [SerializeField] float losingSpeed;

    public override void Move(Enemy enemy)
    {
        Vector3 pos = enemy.rb.position;
        if (enemy.IsLosing)
        {
            MoveLosing(enemy, pos);
            return;
        }

        Vector3 train = enemy.TargetWagon.Middle;
        if (train == null)
        {
            enemy.SetAtTarget(false);
            return;
        }

        float minZ = enemy.Limits.Item1;
        float maxZ = enemy.Limits.Item2;
        bool insideLane = pos.z <= minZ && pos.z >= maxZ;
        float targetX = train.x;
        float distanceToX = Mathf.Abs(pos.x - targetX);
        float stopDistance = 5f;

        enemy.SetAtTarget(insideLane && distanceToX <= stopDistance);
        if (enemy.TimeAtTarget >= timeBeforeLosing)
        {
            enemy.BeginLosing();
            MoveLosing(enemy, pos);
            return;
        }

        // =========================
        // COMPORTAMIENTO NORMAL (mientras no esta perdiendo)
        // =========================
        if (!insideLane)
        {
            Vector3 dir = (train - pos).normalized;
            enemy.rb.MovePosition(pos + dir * speed * Time.deltaTime);
            return;
        }
        else
        {
            if (distanceToX <= stopDistance) return;

            Vector3 lateralDir = pos.x < targetX ? Vector3.right : Vector3.left;
            Vector3 nextPos = pos + lateralDir * speed * Time.deltaTime;
            enemy.rb.MovePosition(nextPos);
            
        }

    }

    private void MoveLosing(Enemy enemy, Vector3 pos)
    {
        Vector3 losingPos = new Vector3(pos.x - losingSpeed * Time.deltaTime, pos.y, pos.z);
        enemy.rb.MovePosition(losingPos);
    }

    public override void Knockback(Enemy enemy)
    {
        enemy.rb.AddForce(enemy.rb.transform.forward * 10, ForceMode.Impulse);
    }
}