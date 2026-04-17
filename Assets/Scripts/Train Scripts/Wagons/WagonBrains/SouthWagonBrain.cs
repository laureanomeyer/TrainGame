using System.Collections.Generic;
using UnityEngine;

public class SouthWagonBrain : WagonBrain
{
    [SerializeField] private float shieldsBonus;

    public override IEnumerable<StatModifier> GetModifiers()
    {
        yield return new StatModifier(StatType.Defense, shieldsBonus, ModifierType.Additive, this);
    }

}
