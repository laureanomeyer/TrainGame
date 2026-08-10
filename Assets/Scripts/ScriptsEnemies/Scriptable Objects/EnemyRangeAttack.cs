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
            enemy.ResetAttackCooldown(enemy.Cooldown);
        }
    }

    public override void Skill(Enemy enemy)
    {
        if (enemy.Target == null) return;
        if (!enemy.CanSkill) return;

        if (enemy.CanSkill && enemy.Skill != null)
        {
            enemy.Skill.Play(enemy);
            enemy.ResetSkillCooldown(enemy.Skill.Cooldown);
        }
    }
}