using System;
using System.Collections.Generic;
using UnityEngine;

public class ArmorSkill : EnemySkill
{
    public GameObject armorPrefab;
    public override void Play(Enemy enemy)
    {
        if (enemy == null)
        {
            return;
        }
        EnemiesController enemies = ServiceLocator.Get<EnemiesController>();
        if (enemies == null) return;

        if (armorPrefab == null)
        {
            Debug.LogError("ArmorSkill requires an Armor prefab.");
            return;
        }

        if (!TrySelectTarget(enemies.ActiveEnemies, enemy, out Enemy target))
            return;

        GameObject armorObject = ObjectPoolManager.SpawnObject(
            armorPrefab,
            enemy.transform.position,
            Quaternion.identity
        );
        if (armorObject == null)
        {
            Debug.LogError("Failed to spawn the Armor projectile.");
            return;
        }

        if (!armorObject.TryGetComponent(out Armor armor))
        {
            Debug.LogError("ArmorSkill prefab must have an Armor component.", armorObject);
            ObjectPoolManager.ReturnObjectToPool(armorObject);
            return;
        }

        if (!armor.SetTarget(target))
        {
            ObjectPoolManager.ReturnObjectToPool(armorObject);
            return;
        }

        Action<OnArcMotionEnded> handler = null;
        handler = eventData =>
        {
            if (eventData.ArcMotion != armor) return;

            EventBus.Unsubscribe(handler);
            if (target != null &&
                target != enemy &&
                target.HealthState == EnemyHealthState.Normal)
            {
                ApplyArmor(target);
            }
        };
        EventBus.Subscribe(handler);
    }

    private static bool TrySelectTarget(
        IReadOnlyCollection<Enemy> activeEnemies,
        Enemy caster,
        out Enemy target)
    {
        target = null;
        if (activeEnemies == null) return false;

        int eligibleCount = 0;
        foreach (Enemy candidate in activeEnemies)
        {
            if (candidate == null ||
                candidate == caster ||
                candidate.HealthState != EnemyHealthState.Normal)
            {
                continue;
            }

            eligibleCount++;
            if (UnityEngine.Random.Range(0, eligibleCount) == 0)
                target = candidate;
        }

        return target != null;
    }

    public override void Stop(Enemy enemy)
    {
        // No specific stop behavior for ArmorSkill
    }

    void ApplyArmor(Enemy enemy)
    {
        if (enemy == null)
        {
            return;
        }

        enemy.Brain.AddArmor(enemy);
    }
}