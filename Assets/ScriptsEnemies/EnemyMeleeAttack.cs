using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Attack/Melee")]
public class EnemyMeleeAttack : EnemyAttackSO
{
    public override void Attack(Enemy enemy)
    {
        Debug.Log("melee attack");
    }
}