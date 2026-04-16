using System.Data;

public enum InteractableType { CoalBox, GoldBox }
public enum ModifierType { Additive, Multipicaive}
public enum StatType { MaxHp, Defense, GoldMultiplier, DamageMultiplier, AttackSpeed, FuelOptimizer, Speed }
public struct TrainStats
{
    public float fuelOptimizer;
    public float trainMaxHp;
    public float shields;
    public float goldBonus;
    public float damageBonus;
    public float attackSpeed;
    public float baseSpeed;
    public TrainStats(float trainMaxHp, float shields, float goldBonus, float damageBonus, float attackSpeed, float fuelOptimizer, float baseSpeed)
    {
        this.trainMaxHp = trainMaxHp;
        this.shields = shields;
        this.goldBonus = goldBonus;
        this.damageBonus = damageBonus;
        this.attackSpeed = attackSpeed;
        this.fuelOptimizer = fuelOptimizer;
        this.baseSpeed = baseSpeed;
    }
    public TrainStats(LocomotiveStatsSO trainData)
    {
        trainMaxHp = trainData.maxHp;
        shields = trainData.defense;
        goldBonus = trainData.goldMultyplier;
        damageBonus = trainData.damageMultyplier;
        attackSpeed = trainData.attackSpeed;
        fuelOptimizer = trainData.fuelOptimizer;
        baseSpeed = trainData.baseSpeed;
    }


    //Funcion encargada de sumar las varibles de dos TrainsStats
    public static TrainStats operator +(TrainStats x, TrainStats y)
    {
        return new TrainStats
        {
            fuelOptimizer = x.fuelOptimizer + y.fuelOptimizer,
            trainMaxHp = x.trainMaxHp + y.trainMaxHp,
            shields = x.shields + y.shields,
            goldBonus = x.goldBonus + y.goldBonus,
            damageBonus = x.damageBonus + y.damageBonus,
            attackSpeed = x.attackSpeed + y.attackSpeed,
            baseSpeed = x.baseSpeed + y.baseSpeed
        };
    }
    public static TrainStats operator -(TrainStats x, TrainStats y)
    {
        return new TrainStats
        {
            fuelOptimizer = x.fuelOptimizer - y.fuelOptimizer,
            trainMaxHp = x.trainMaxHp - y.trainMaxHp,
            shields = x.shields - y.shields,
            goldBonus = x.goldBonus - y.goldBonus,
            damageBonus = x.damageBonus - y.damageBonus,
            attackSpeed = x.attackSpeed - y.attackSpeed,
            baseSpeed = x.baseSpeed - y.baseSpeed
        };
    }
}

public readonly struct StatModifier
{
    public readonly StatType StatType;
    public readonly float Value;
    public readonly ModifierType ModifierType;
    public readonly object Source;

    public StatModifier(StatType stat, float value, ModifierType type, object source)
    {
        StatType = stat;
        Value = value;
        ModifierType = type;
        Source = source;
    }
}

