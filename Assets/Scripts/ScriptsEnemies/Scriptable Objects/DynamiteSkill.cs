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
        if (enemy.Target == null) return;

        IWagon targetWagon = FindWagonByTransform(enemy);
        if (targetWagon == null) return;

        activeDynamite = ObjectPoolManager.Instantiate(dynamitePrefab);
        activeDynamite.transform.position = enemy.transform.position;

        var dn = activeDynamite.GetComponent<Dynamite>();
        dn.SetTarget(targetWagon, enemy.TargetList, damage, adjacentDamageMultiplier);
    }

    public override void Stop(Enemy enemy)
    {
        return;
    }

    private IWagon FindWagonByTransform(Enemy enemy)
    {
        for (int i = 0; i < enemy.TargetList.Count; i++)
        {
            if (enemy.TargetList[i].Transform == enemy.Target)
                return enemy.TargetList[i];
        }
        return null;
    }
}