using UnityEngine;

public class LowHeatWagonBrain : WagonBrain
{
    [SerializeField] private float optimizer;

    public override TrainStats GetStatsBuff(LocomotiveStats baseStats)
    {
        return new TrainStats(
            0,
            0,
            0,
            0,
            0,
            baseStats.fuelOptimizer * optimizer,
            0
        );
    }
}