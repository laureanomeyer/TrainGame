using UnityEngine;

public abstract class EnemyMovementSO : ScriptableObject
{
    public float speed;
    public abstract void Knockback(Enemy enemy);

    public abstract void Move(Enemy enemy);
}
