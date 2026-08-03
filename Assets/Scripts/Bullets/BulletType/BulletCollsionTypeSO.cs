using UnityEngine;

public abstract class BulletCollsionTypeSO : ScriptableObject
{
    public abstract void BulletCollision(Enemy enemy, BulletScript bulletInfo);
}
