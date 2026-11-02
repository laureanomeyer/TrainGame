using System;
using UnityEngine;

public class ShopPassiveWagonData : ShopWagonData
{
    [SerializeField] private StatType statUpgrade;
    [SerializeField] private float upgradeAmount;

    public StatModifier ApplyPassiveStats()
    {
        return new StatModifier(statUpgrade, upgradeAmount, ModifierType.Additive, this);
    }
}
