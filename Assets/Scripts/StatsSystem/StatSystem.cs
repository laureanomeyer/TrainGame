using System;
using System.Collections.Generic;

public class StatSystem
{
    private readonly LocomotiveStatsSO baseStats;
    private readonly TrainStats locoMultipliers;
    private readonly List<StatModifier> modifiers = new();
    
    public event Action<StatType, float> OnStatChanged;

    public StatSystem(LocomotiveStatsSO baseStats, TrainStats locoMultipliers)
    {
        this.baseStats = baseStats;
        this.locoMultipliers = locoMultipliers;
    }

    public void AddModifier(StatModifier mod)
    {
        modifiers.Add(mod);
        OnStatChanged?.Invoke(mod.StatType, GetStat(mod.StatType));
    }

    public void RemoveModifiersFromSource(object source)
    {
        int removed = modifiers.RemoveAll(m => m.Source == source);
        if (removed > 0) RecalculateAll();
    }

    public float GetStat(StatType type)
    {
        float baseValue = GetBase(type) * GetLocoMultiplier(type);
        float additive = 0f;
        float multiplicative = 1f;

        foreach (var mod in modifiers)
        {
            if (mod.StatType != type) continue;
            if (mod.ModifierType == ModifierType.Additive) additive += mod.Value;
            else if (mod.ModifierType == ModifierType.Multipicaive) multiplicative *= mod.Value;
        }

        return (baseValue + additive) * multiplicative;
    }

    private void RecalculateAll()
    {
        foreach (StatType type in Enum.GetValues(typeof(StatType)))
            OnStatChanged?.Invoke(type, GetStat(type));
    }

    private float GetBase(StatType type) => type switch
    {
        StatType.MaxHp => baseStats.maxHp,
        StatType.Defense => baseStats.defense,
        StatType.GoldMultiplier => baseStats.goldMultyplier,
        StatType.DamageMultiplier => baseStats.damageMultyplier,
        StatType.AttackSpeed => baseStats.attackSpeed,
        StatType.FuelOptimizer => baseStats.fuelOptimizer,
        StatType.Speed => baseStats.baseSpeed,
        _ => 1f
    };

    private float GetLocoMultiplier(StatType type) => type switch
    {
        StatType.MaxHp => locoMultipliers.trainMaxHp,
        StatType.Defense => locoMultipliers.shields,
        StatType.GoldMultiplier => locoMultipliers.goldBonus,
        StatType.DamageMultiplier => locoMultipliers.damageBonus,
        StatType.AttackSpeed => locoMultipliers.attackSpeed,
        StatType.FuelOptimizer => locoMultipliers.fuelOptimizer,
        StatType.Speed => locoMultipliers.baseSpeed,
        _ => 1f
    };
}

