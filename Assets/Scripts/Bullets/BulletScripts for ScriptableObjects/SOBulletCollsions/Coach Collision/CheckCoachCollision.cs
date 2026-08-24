using UnityEngine;

[CreateAssetMenu(fileName = "CheckCoachCollisionSO", menuName = "Weapons/Type of collsion/Check Coach collision")]
public class CheckCoachCollision : BulletCollsionTypeSO
{
    public override void BulletCollision(Enemy enemy, BulletScript bulletInfo)
    {
        bool enemyDead = enemy.TakeDamage(bulletInfo.Damage);

        if (enemyDead)
        {
            EventBus.Publish(new OnCoachDetectedDeadEnemy(1));
        }

        if (bulletInfo.DestroyOnEnemy)
        {
            bulletInfo.Deactivate();
            return;
        }
    }
}
