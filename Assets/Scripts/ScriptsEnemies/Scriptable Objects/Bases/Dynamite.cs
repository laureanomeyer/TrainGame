using UnityEngine;
using System.Collections.Generic;

public class Dynamite : MonoBehaviour
{
    [SerializeField] float speed = 5f;
    [SerializeField] float arcHeight = 2f;

    private IWagon targetWagon;
    private List<IWagon> allWagons;
    private float damage;
    private float adjacentDamageMultiplier;

    private ArcMover mover;

    public void SetTarget(IWagon target, List<IWagon> wagons, float dmg, float adjacentMult)
    {
        targetWagon = target;
        allWagons = wagons;
        damage = dmg;
        adjacentDamageMultiplier = adjacentMult;

        mover = new ArcMover(transform.position, target.Transform, speed, arcHeight);
    }

    void Update()
    {
        Move();
    }

    private void Move()
    {
        if (targetWagon == null || targetWagon.Transform == null)
        {
            ObjectPoolManager.ReturnObjectToPool(gameObject);
            return;
        }

        transform.position = mover.Tick(Time.deltaTime);

        if (mover.IsFinished)
        {
            DoDamage();
            ObjectPoolManager.ReturnObjectToPool(gameObject);
        }
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
        if (wagon == null || wagon.Transform == null) return;

        var damagable = wagon.Transform.GetComponent<IDamagable>();
        damagable?.TakeDamage(amount);
    }
}