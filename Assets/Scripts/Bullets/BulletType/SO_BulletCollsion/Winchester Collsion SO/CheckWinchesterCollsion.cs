using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "CheckWinchesterCollisionSO", menuName = "Weapons/Type of collsion/Check winchester collision")]

public class CheckWinchesterCollsion : BulletCollsionTypeSO
{
    public List<Enemy> enemiesDamage;

    public override void BulletCollision(Enemy enemy, BulletScript bulletInfo)
    {
        bool enemyDead = enemy.TakeDamage(bulletInfo.Damage);

        if (!enemyDead)
        {
            enemiesDamage.Add(enemy);
            EventBus.Publish(new OnUpdateEnemiesDamage(enemiesDamage));
        }
        else
        {
            if (enemiesDamage.Contains(enemy))
            {
                Debug.Log("El enemigo fue atacado antes");
                enemiesDamage.Remove(enemy);
                EventBus.Publish(new OnUpdateEnemiesDamage(enemiesDamage));
            }
            else
            {
                Debug.Log("Enemigo oneshoteado");
                EventBus.Publish(new OnUpdateWinchesterLegadoLeftPoint(1));
            }
        }

        if (bulletInfo.DestroyOnEnemy)
        {
            bulletInfo.Deactivate();
            return;
        }
    }
}
