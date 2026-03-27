using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Attack/Ranged")]


public class EnemyRangeAttack : EnemyAttackSO
{

    public override void Attack(Enemy enemy)
    {
        enemy.Weapon.Execute(enemy.Target);
    }
}
