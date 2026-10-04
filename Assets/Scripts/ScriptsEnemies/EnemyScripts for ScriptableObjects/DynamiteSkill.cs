using System;
using UnityEngine;

[Serializable]
public class DynamiteSkill : EnemySkill
{
    public float damage = 20f;
    public float adjacentDamageMultiplier = 0.5f;
    public GameObject dynamitePrefab;
    private GameObject activeDynamite;

    public override void Play(Enemy enemy)
    {
        if (enemy.TargetWagon == null) return;

        IWagon targetWagon = enemy.TargetWagon;
        if (targetWagon == null) return;

        activeDynamite = ObjectPoolManager.SpawnObject(
            dynamitePrefab,
            enemy.transform.position,
            Quaternion.identity
        );

        var dn = activeDynamite.GetComponent<Dynamite>();
        dn.SetTarget(targetWagon, RunManager.Instance.ActiveWagons, damage, adjacentDamageMultiplier);
    }

    public override void Stop(Enemy enemy)
    {
        return;
    }
}