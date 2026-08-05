using UnityEngine;

[CreateAssetMenu(fileName = "New collsion", menuName = "Weapons/Type of collsion/winchester bullet collsion")]
public class WinchestertCollsionSO : BulletCollsionTypeSO
{
    public override void BulletCollision(Enemy enemy, BulletScript bulletInfo)
    {
        bool enemyDead = enemy.TakeDamage(bulletInfo.Damage);

        if (enemyDead)
        {
            EventBus.Publish(new OnDetectedDeadEnemy());
        }

        if (bulletInfo.DestroyOnEnemy)
        {
            bulletInfo.Deactivate();
            return;
        }
    }
}
