using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Attack/Ranged")]


public class EnemyRangeAttack : EnemyAttackSO
{

    public override void Attack(Enemy enemy)
    {
        if (enemy.Target == null) return;

        float dist = Vector3.Distance(
            enemy.transform.position,
            enemy.Target.transform.position
        );

        if (dist <= enemy.Range)
        {
            enemy.Weapon.Execute(enemy.Target);
        }
    }
}
