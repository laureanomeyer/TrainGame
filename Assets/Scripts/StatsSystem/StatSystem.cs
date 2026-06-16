using System;
using System.Collections.Generic;
using UnityEngine;

public class StatSystem
{
    private readonly LocomotiveStatsSO baseStats;
    private readonly TrainData trainData;
    private readonly List<StatModifier> modifiers = new();
    
    public event Action<StatType, float> OnStatChanged;

    public StatSystem(LocomotiveStatsSO baseStats, TrainData trainData)
    {
        this.baseStats = baseStats;
        this.trainData = trainData;
    }

    public void AddModifier(StatModifier mod)
    {
        modifiers.Add(mod);
        OnStatChanged?.Invoke(mod.StatType, GetStat(mod.StatType));
        GameEvents.StatChanged();
    }

    public void RemoveModifiersFromSource(object source)
    {
        int removed = modifiers.RemoveAll(m => m.Source == source);
        if (removed > 0) RecalculateAll();
    }

    public float GetStat(StatType type)
    {
        float baseValue = GetBase(type);
        float locoMultiplier = GetLocoMultiplier(type);

        float extra = 0f;

        if (type != StatType.FuelOptimizer)
        {
            foreach (var mod in modifiers)
            {
                if (mod.StatType != type) continue;
                if (mod.ModifierType == ModifierType.Additive)
                {
                    extra += locoMultiplier * mod.Value;
                }
            }
        }

        else
        {
            extra = locoMultiplier;
            foreach (var mod in modifiers)
            {
                if (mod.StatType != type) continue;
                if (mod.ModifierType == ModifierType.Additive)
                {
                    extra += extra * mod.Value;
                }
            }
            return extra;
        }
        return baseValue * (locoMultiplier + extra);
    }

    public void RecalculateAll()
    {
        foreach (StatType type in Enum.GetValues(typeof(StatType)))
        {
            OnStatChanged?.Invoke(type, GetStat(type));
            GameEvents.StatChanged();
        }
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

    public float GetLocoMultiplier(StatType type) => type switch
    {
        StatType.MaxHp => trainData.LocomotiveStatsMultiplicator.trainMaxHp,
        StatType.Defense => trainData.LocomotiveStatsMultiplicator.shields,
        StatType.GoldMultiplier => trainData.LocomotiveStatsMultiplicator.goldBonus,
        StatType.DamageMultiplier => trainData.LocomotiveStatsMultiplicator.damageBonus,
        StatType.AttackSpeed => trainData.LocomotiveStatsMultiplicator.attackSpeed,
        StatType.FuelOptimizer => trainData.LocomotiveStatsMultiplicator.fuelOptimizer,
        StatType.Speed => trainData.LocomotiveStatsMultiplicator.baseSpeed,
        _ => 1f
    };
}