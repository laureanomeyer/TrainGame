using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Attack/Ranged")]
public class EnemyRangeAttack : EnemyAttackSO
{

    public override void Attack(Enemy enemy)
    {
        try
        {
            if (enemy.Target == null) return;

            if (!enemy.CanAttack) return;

            float dist = Vector3.Distance(
                enemy.transform.position,
                enemy.Target.position
            );

            if (dist <= enemy.Range + 5)
            {
                enemy.Weapon.Execute(enemy.Target, enemy.Damage);
                enemy.ResetAttackCooldown(enemy.Cooldown);
            }
        }
        catch
        {
            return;
        }
        
            
    }
}