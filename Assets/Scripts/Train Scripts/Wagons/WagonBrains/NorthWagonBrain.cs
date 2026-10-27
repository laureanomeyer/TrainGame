using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

public class NorthWagonBrain : WagonBrain
{
    [SerializeField] private float dmgBonus;

    private void Awake()
    {
        WagonType = WagonType.Passive;
    }

    public override IEnumerable<List<StatModifier>> GetModifiers()
    {
        List<StatModifier> mods = new List<StatModifier>(); ;
        mods.Add(new StatModifier(StatType.DamageMultiplier, dmgBonus, ModifierType.Additive, this));
        yield return mods;
    }

}
