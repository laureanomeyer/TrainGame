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

        IWagon targetWagon = FindWagonByTransform(enemy);
        if (targetWagon == null) return;

        activeDynamite = ObjectPoolManager.Instantiate(dynamitePrefab);
        activeDynamite.transform.position = enemy.transform.position;

        var dn = activeDynamite.GetComponent<Dynamite>();
        dn.SetTarget(targetWagon, RunManager.Instance.ActiveWagons, damage, adjacentDamageMultiplier);
    }

    public override void Stop(Enemy enemy)
    {
        return;
    }

    private IWagon FindWagonByTransform(Enemy enemy)
    {
        for (int i = 0; i < RunManager.Instance.ActiveWagons.Count; i++)
        {
            if (RunManager.Instance.ActiveWagons[i]?.Middle == enemy.TargetWagon.Middle)
                return RunManager.Instance.ActiveWagons[i];
        }
        return null;
    }
}