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

        Vector3 pos = enemy.rb.transform.position;

        if (train == null)
            return;

        enemy.rb.MovePosition(enemy.transform.position + (train.position - enemy.transform.position).normalized * enemy.Speed * Time.deltaTime);

        if (enemy.transform.position.z > TrainRanges.negativeLimit || enemy.transform.position.z < TrainRanges.positiveLimit)
        {

        }

        if (pos.y < 0)
            pos.y = 0;

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