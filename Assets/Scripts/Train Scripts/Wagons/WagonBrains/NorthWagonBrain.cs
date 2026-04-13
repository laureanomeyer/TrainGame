using UnityEngine;

public class NorthWagonBrain : WagonBrain
{
    [SerializeField] private float dmgBonus;

    public override TrainStats GetStatsBuff(LocomotiveStats baseStats)
    {
        return new TrainStats(
            0,
            0,
            0,
            baseStats.damageMultyplier * dmgBonus,
            0,
            0,
            0
        );
    }

}
