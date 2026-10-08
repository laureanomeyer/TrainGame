using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Movement/Slow")]
public class EnemyMovementSlow : EnemyMovementSO
{
    public override void Move(Enemy enemy)
    {
        Vector3 pos = enemy.rb.position;
        Vector3 nextPos = pos;

        if (enemy.TargetWagon != null)
        {
            Vector3 train = enemy.TargetWagon.Middle;
            float minZ = enemy.Limits.Item1;
            float maxZ = enemy.Limits.Item2;
            bool insideLane = pos.z <= minZ && pos.z >= maxZ;
            Vector3 targetWithoutPositiveX = train;
            targetWithoutPositiveX.x = Mathf.Min(pos.x, train.x);

            if (insideLane)
            {
                float targetX = Mathf.Min(pos.x, train.x);
                nextPos.x = Mathf.MoveTowards(pos.x, targetX, speed * Time.deltaTime);
            }
            else
            {
                targetWithoutPositiveX.z = Mathf.Clamp(train.z, maxZ, minZ);
                nextPos = Vector3.MoveTowards(pos, targetWithoutPositiveX, speed * Time.deltaTime);
            }
        }

        nextPos.x -= speed * Time.deltaTime;
        enemy.rb.MovePosition(nextPos);
    }

    public override void Knockback(Enemy enemy)
    {
        enemy.rb.AddForce(enemy.rb.transform.forward * 10, ForceMode.Impulse);
    }

    public override void Begin(Enemy enemy)
    {
        enemy.ChangeState(EnemyMovementState.Losing);
    }

}