using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Attack")]

public class EnemyAttackSO : ScriptableObject
{
    public void Attack(Enemy enemy)
    {
        RangedAttack(enemy);
    }

    public void Skill(Enemy enemy)
    {
        if (enemy.TargetWagon == null) return;
        if (!enemy.CanSkill) return;

        if (enemy.CanSkill && enemy.Skill != null && IsTargetInRange(enemy))
        {
            enemy.Skill.Play(enemy);
            enemy.ResetSkillCooldown(enemy.Skill.Cooldown);
        }
    }

    private void RangedAttack(Enemy enemy)
    {
        if (enemy.TargetWagon == null) return;
        if (!enemy.CanAttack) return;

        if (IsTargetInRange(enemy))
        {
            enemy.PlayAttackAnimation();
            if (enemy.TargetWagon == null) return;

            enemy.Weapon.Execute(enemy.TargetWagon, enemy.Damage);
            enemy.ResetAttackCooldown(enemy.Cooldown);

        }
    }

    private bool IsTargetInRange(Enemy enemy)
    {
        if (enemy.TargetWagon == null) return false;

        float dist = Vector3.Distance(enemy.transform.position, enemy.TargetWagon.Middle);
        return dist <= enemy.Range + 5;
    }


}
