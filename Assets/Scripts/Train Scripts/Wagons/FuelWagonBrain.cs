using UnityEngine;

public class FuelWagonBrain : WagonBrain
{
    [SerializeField] private float fuelOpt;
    new void Start()
    {
        base.Start();
        base.statsBuff = new TrainStats(fuelOpt,0,0,0,0,0,fuelOpt,0);

    }

}
