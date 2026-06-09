using UnityEngine;

public interface IEnemyMovement
{
    void Move(Enemy enemy);

    void Knockback(Enemy enemy);
}
