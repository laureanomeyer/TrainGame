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

    public override IEnumerable<List<StatModifier>> GetModifiers()
    {
        List<StatModifier> mods = new List<StatModifier>(); ;
        mods.Add(new StatModifier(StatType.Defense, resBonus, ModifierType.Additive, this));
        mods.Add(new StatModifier(StatType.FuelOptimizer, -optDebuff, ModifierType.Additive, this));

        yield return mods;
    }
}
