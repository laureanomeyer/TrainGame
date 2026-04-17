using System.Collections.Generic;
using UnityEngine;

public class NorthWagonBrain : WagonBrain
{
    [SerializeField] private float dmgBonus;

    public override IEnumerable<StatModifier> GetModifiers()
    {
        yield return new StatModifier(StatType.DamageMultiplier, dmgBonus, ModifierType.Additive, this);
    }

}
