using UnityEngine;

public abstract class EnemyBrainSO : ScriptableObject, IEnemyBrain
{
    public abstract void Begin(Enemy enemy);

    public abstract Transform SetTarget(Enemy enemy);

    public abstract void Tick(Enemy enemy);
}
