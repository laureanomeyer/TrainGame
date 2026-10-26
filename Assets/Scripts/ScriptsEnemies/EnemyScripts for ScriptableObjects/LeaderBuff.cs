using System;
using System.Text;
using NUnit.Framework;
using UnityEngine;
[Serializable]
public class LeaderBuff : EnemySkill
{
    public float damageIncrease;
    public GameObject LeaderParticles;
    private EnemiesController enemies;
    private bool isActive = false;
    public override void Play(Enemy enemy)
    {
        LeaderParticles.SetActive(false);
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
        LeaderParticles.SetActive(true);
    }

    public override void Stop(Enemy enemy)
    {
        foreach(Enemy en in enemies.ActiveEnemies)
        {
            if (en != enemy)
                en.Brain.DeBuff(en, damageIncrease);
        }
        LeaderParticles.SetActive(false);
    }
}