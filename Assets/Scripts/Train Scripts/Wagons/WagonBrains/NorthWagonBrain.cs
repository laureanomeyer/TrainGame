using System.Collections.Generic;
using UnityEngine;

public class NorthWagonBrain : WagonBrain
{
    [SerializeField] private float dmgBonus;

    private void Awake()
    {
        WagonType = WagonType.Passive;
    }

    public override IEnumerable<StatModifier> GetModifiers()
    {
        yield return new StatModifier(StatType.DamageMultiplier, dmgBonus, ModifierType.Additive, this);
    }

}
