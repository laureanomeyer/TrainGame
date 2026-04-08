using UnityEngine;

public abstract class EnemyMovementSO : ScriptableObject, IEnemyMovement
{
    public abstract void Move(Enemy enemy);
}
