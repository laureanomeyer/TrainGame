using System;
using System.Collections;
using UnityEngine;
[Serializable]
public class LeaderBuff : EnemySkill
{
    public float damageIncrease;
    public GameObject LeaderParticles;
    private GameObject CurrentParticles;
    private EnemiesController enemies;
    public override void Begin(Enemy enemy)
    {
        CurrentParticles = ObjectPoolManager.SpawnObject(LeaderParticles, enemy.transform.position, enemy.transform.rotation);
        CurrentParticles.SetActive(false);
    }


    public override void Play(Enemy enemy)
    {
        CurrentParticles.SetActive(true);
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
        StopParticles();

    }

    public override void Stop(Enemy enemy)
    {
        foreach(Enemy en in enemies.ActiveEnemies)
        {
            if (en != enemy)
                en.Brain.DeBuff(en, damageIncrease);
        }
        CurrentParticles.SetActive(false);
    }

    private void StopParticles()
    {
        float elapsed = 0f;
        float duration = cooldown / 2f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
        }

        CurrentParticles.SetActive(false);
    }

}