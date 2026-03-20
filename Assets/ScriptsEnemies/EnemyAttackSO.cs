using UnityEngine;

public abstract class EnemyAttackSO : ScriptableObject, IEnemyAttack
{
    public abstract void Attack(Enemy enemy);
}
