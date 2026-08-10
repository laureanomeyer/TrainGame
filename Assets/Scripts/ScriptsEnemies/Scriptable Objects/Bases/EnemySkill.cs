using System;
[System.Serializable]
public abstract class EnemySkill
{
    public float cooldown;

    public abstract void Play(Enemy enemy);
    public abstract void Stop(Enemy enemy);

}