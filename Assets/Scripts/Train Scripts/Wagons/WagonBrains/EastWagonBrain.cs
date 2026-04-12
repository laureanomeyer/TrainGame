using UnityEngine;

public class EastWagonBrain : WagonBrain
{
    [SerializeField] private float hpBonus;

    public override TrainStats GetStatsBuff(LocomotiveStats baseStats)
    {
        return new TrainStats(
            baseStats.maxHp * hpBonus,
            0,
            0,
            0,
            0,
            0,
            0
        );
    }
}