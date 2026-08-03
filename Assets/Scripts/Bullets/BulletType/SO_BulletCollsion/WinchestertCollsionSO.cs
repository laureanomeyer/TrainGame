using UnityEngine;

[CreateAssetMenu(fileName = "New collsion", menuName = "Weapons/Type of collsion/winchester bullet collsion")]
public class WinchestertCollsionSO : BulletCollsionTypeSO
{
    public override void BulletCollision(Enemy enemy, BulletScript bulletInfo)
    {
        enemy.TakeDamage(bulletInfo.Damage);

        if (bulletInfo.DestroyOnEnemy)
        {
            bulletInfo.Deactivate();
            return;
        }
    }
}
