using System.Collections.Generic;
using UnityEngine;

public class HighHeatBrain : WagonBrain
{
    [SerializeField] private float resBonus;
    [SerializeField] private float optDebuff;

    private void Awake()
    {
        WagonType = WagonType.Passive;
    }

    public override IEnumerable<StatModifier> GetModifiers()
    {
        yield return new StatModifier(StatType.DamageMultiplier, resBonus, ModifierType.Additive, this);
    }
}
