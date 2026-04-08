using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Attack/Ranged")]
public class EnemyRangeAttack : EnemyAttackSO
{
    [SerializeField] float cooldown = 1f;

    public override void Attack(Enemy enemy)
    {
        if (enemy.Target == null) return;

        if (!enemy.CanAttack) return;

        float dist = Vector3.Distance(
            enemy.transform.position,
            enemy.Target.position
        );

        if (dist <= enemy.Range)
        {
            enemy.Weapon.Execute(enemy.Target);
            enemy.ResetAttackCooldown(cooldown);
        }
    }
}