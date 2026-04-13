using UnityEngine;

public abstract class EnemyMovementSO : ScriptableObject, IEnemyMovement
{
    public abstract void Move(Enemy enemy, float limitZ);

    public abstract float SetLimitZ();
}
