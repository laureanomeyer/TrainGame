using System.Collections.Generic;
using UnityEngine;

public class SouthWagonBrain : WagonBrain
{
    [SerializeField] private float shieldsBonus;

    private void Awake()
    {
        WagonType = WagonType.Passive;
    }

    public override IEnumerable<List<StatModifier>> GetModifiers()
    {
        List<StatModifier> mods = new List<StatModifier>(); ;
        mods.Add(new StatModifier(StatType.Defense, shieldsBonus, ModifierType.Additive, this));
        yield return mods;
    }

}
