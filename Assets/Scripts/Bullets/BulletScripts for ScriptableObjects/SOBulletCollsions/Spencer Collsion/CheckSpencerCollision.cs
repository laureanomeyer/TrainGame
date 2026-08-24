using UnityEngine;


[CreateAssetMenu(fileName = "CheckSpencerCollisionSO", menuName = "Weapons/Type of collsion/Check Spencer collision")]
public class CheckSpencerCollision : BulletCollsionTypeSO
{
    public override void BulletCollision(Enemy enemy, BulletScript bulletInfo)
    {
        bool enemyDead = enemy.TakeDamage(bulletInfo.Damage);

        if (enemyDead)
        {
            EventBus.Publish(new OnSpencerDetectedDeadEnemy());
        }

        if (bulletInfo.DestroyOnEnemy)
        {
            bulletInfo.Deactivate();
            return;
        }
    }
}
