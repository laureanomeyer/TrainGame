using System.Collections.Generic;
using UnityEngine;

public class LowHeatWagonBrain : WagonBrain
{
    [SerializeField] private float optimizer;

    private void Awake()
    {
        WagonType = WagonType.Passive;
    }

    public override IEnumerable<List<StatModifier>> GetModifiers()
    {
        List<StatModifier> mods = new List<StatModifier>(); ;
        mods.Add(new StatModifier(StatType.FuelOptimizer, optimizer, ModifierType.Additive, this));
        yield return mods;
    }
}