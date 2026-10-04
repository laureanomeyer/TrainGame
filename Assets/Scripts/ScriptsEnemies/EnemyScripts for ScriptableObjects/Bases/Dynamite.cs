using UnityEngine;
using System.Collections.Generic;

public class Dynamite : ArcMotion
{
    [SerializeField] float speed = 5f;
    [SerializeField] float arcHeight = 2f;

    private IWagon targetWagon;
    private List<IWagon> allWagons;
    private float damage;
    private float adjacentDamageMultiplier;

    public void SetTarget(IWagon target, List<IWagon> wagons, float dmg, float adjacentMult)
    {
        targetWagon = target;
        allWagons = wagons;
        damage = dmg;
        adjacentDamageMultiplier = adjacentMult;

        BeginArcMotion(target.Middle, speed, arcHeight);
    }

    protected override void Update()
    {
        if (targetWagon == null || targetWagon.Head == null)
        {
            ObjectPoolManager.ReturnObjectToPool(gameObject);
            return;
        }

        base.Update();
    }

    protected override void OnArcMotionCompleted()
    {
        DoDamage();
        ObjectPoolManager.ReturnObjectToPool(gameObject);
    }

    private void DoDamage()
    {
        ApplyDamage(targetWagon, damage);

        int index = allWagons.IndexOf(targetWagon);
        if (index < 0) return;

        if (index - 1 >= 0)
            ApplyDamage(allWagons[index - 1], damage * adjacentDamageMultiplier);

        if (index + 1 < allWagons.Count)
            ApplyDamage(allWagons[index + 1], damage * adjacentDamageMultiplier);
    }

    private void ApplyDamage(IWagon wagon, float amount)
    {
        if (wagon is IDamagable damagable)
        {
            damagable.TakeDamage(amount);
        }
    }
}