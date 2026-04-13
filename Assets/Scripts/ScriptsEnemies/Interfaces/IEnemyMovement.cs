using UnityEngine;

public interface IEnemyMovement
{

    float SetLimitZ();
    void Move(Enemy enemy, float limitZ);
}
