using UnityEngine;

public class FuelWagonBrain : WagonBrain
{
    [SerializeField] private LocomotiveStats stats;
    new void Awake()
    {
        base.statsBuff = new TrainStats(stats);
    }

}
