using System;
using System.Text;
using NUnit.Framework;
[Serializable]
public class LeaderBuff : EnemySkill
{
    public float damageIncrease;
    private EnemiesController enemies;
    private bool isActive = false;
    public override void Play(Enemy enemy)
    {
        if (enemy.HealthState == EnemyHealthState.Dead) return;
        enemies = ServiceLocator.Get<EnemiesController>();

        foreach (Enemy en in enemies.ActiveEnemies)
        {
            if (en.AttackState != EnemyAttackState.Buffed)
            {
                if (en != enemy)
                    en.Brain.AddBuff(en, damageIncrease);
            }
        }
    }

    public override void Stop(Enemy enemy)
    {
        foreach(Enemy en in enemies.ActiveEnemies)
        {
            if (en != enemy)
                en.Brain.DeBuff(en, damageIncrease);
        }
    }
}