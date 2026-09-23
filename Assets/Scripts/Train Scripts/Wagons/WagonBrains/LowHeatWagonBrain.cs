using System.Collections.Generic;
using UnityEngine;

public class LowHeatWagonBrain : WagonBrain
{
    [SerializeField] private float optimizer;

    private void Awake()
    {
        WagonType = WagonType.Passive;
    }

    public override IEnumerable<StatModifier> GetModifiers()
    {
        yield return new StatModifier(StatType.FuelOptimizer, optimizer, ModifierType.Additive, this);
    }
}