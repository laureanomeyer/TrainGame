using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EastWagonBrain : WagonBrain
{
    [SerializeField] private float hpBonus;
    public override IEnumerable<StatModifier> GetModifiers()
    {
        yield return new StatModifier(StatType.MaxHp, hpBonus, ModifierType.Additive, this);
    }
}