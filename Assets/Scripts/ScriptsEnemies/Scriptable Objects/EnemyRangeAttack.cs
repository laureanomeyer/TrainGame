using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Attack/Ranged")]
public class EnemyRangeAttack : EnemyAttackSO
{
    public override void Attack(Enemy enemy)
    {
        if (enemy.Target == null) return;
        if (!enemy.CanAttack) return;

        float dist = Vector3.Distance(enemy.transform.position, enemy.Target.position);

        if (dist <= enemy.Range + 5)
        {
            enemy.Weapon.Execute(enemy.Target, enemy.Damage);
            enemy.Skill.Play(enemy);
            enemy.ResetAttackCooldown(enemy.Cooldown);
        }
    }
}