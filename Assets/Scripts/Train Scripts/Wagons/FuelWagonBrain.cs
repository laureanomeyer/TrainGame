using UnityEngine;

public class FuelWagonBrain : WagonBrain
{
    [SerializeField] private float fuelOpt;
    new void Awake()
    {
        base.statsBuff = new TrainStats(fuelOpt,0,0,0,0,0,0,0);

    }

}
