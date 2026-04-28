using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Movement/Normal")]
public class EnemyMovementNormal : EnemyMovementSO
{
    private float desiredDistance = 5f;
    private float tolerance = 0.5f;

    private Transform train;

    public override float SetLimitZ()
    {
        return Random.Range(-3, -1.5f);
    }

    public override void Move(Enemy enemy, float limitZ)
    {
        train = enemy.Target;

        if (train == null)
            return;

        Vector3 pos = enemy.transform.position;

        float stopZ = limitZ + enemy.Range;

        Vector3 dirToTarget = (train.position - pos).normalized;

        Vector3 targetOnLine = train.position;
        targetOnLine.z = stopZ;

        Vector3 dirOnLine = targetOnLine - pos;
        dirOnLine.z = 0;

        if (dirOnLine.sqrMagnitude > 0.01f)
            dirOnLine.Normalize();

        float distance = Vector3.Distance(pos, train.position);

        float t = Mathf.InverseLerp(desiredDistance + tolerance + 1f, desiredDistance, distance);

        Vector3 finalDir = Vector3.Lerp(dirToTarget, dirOnLine, t).normalized;

        pos += finalDir * enemy.Speed * Time.deltaTime;

        // clamp línea
        if (pos.z < stopZ)
            pos.z = stopZ;

        if (pos.y < 0)
            pos.y = 0;
        
        enemy.transform.position = pos;

        if (enemy.IsKnocked)
        {
            Knockback(enemy);
            return;
        }
    }
    public override void Knockback(Enemy enemy)
    {
        Vector3 pos = enemy.transform.position;

        pos += enemy.KnockbackVelocity * Time.deltaTime;

        enemy.transform.position = pos;

        enemy.KnockbackVelocity =
            Vector3.Lerp(
                enemy.KnockbackVelocity,
                Vector3.zero,
                10f * Time.deltaTime
            );

        if (enemy.KnockbackVelocity.magnitude < .1f)
        {
            enemy.KnockbackVelocity = Vector3.zero;
            enemy.IsKnocked = false;
        }
    }
}