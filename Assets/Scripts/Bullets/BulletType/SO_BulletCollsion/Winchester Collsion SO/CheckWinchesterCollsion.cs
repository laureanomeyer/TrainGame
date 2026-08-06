using UnityEngine;


[CreateAssetMenu(fileName = "CheckWinchesterCollisionSO", menuName = "Weapons/Type of collsion/Check winchester collision")]

public class CheckWinchesterCollsion : BulletCollsionTypeSO
{
    public override void BulletCollision(Enemy enemy, BulletScript bulletInfo)
    {
        bool enemyDead = enemy.TakeDamage(bulletInfo.Damage);

        if (enemyDead)
        {
            EventBus.Publish(new OnUpdateWinchesterLegadoLeftPoint(1));
        }

        if (bulletInfo.DestroyOnEnemy)
        {
            bulletInfo.Deactivate();
            return;
        }
    }
}
