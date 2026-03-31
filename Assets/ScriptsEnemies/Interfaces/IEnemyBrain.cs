using UnityEngine;

public interface IEnemyBrain
{
    void Begin(Enemy enemy);
    void Tick(Enemy enemy);

    Transform SetTarget(Enemy enemy);
}