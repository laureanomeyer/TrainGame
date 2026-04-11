using UnityEngine;

public class FuelWagonBrain : WagonBrain
{
    [SerializeField] private float fuelOpt;
    public void Awake()
    {
        base.statsBuff = new TrainStats(fuelOpt,0,0,0,0,0,0,0);

    }

}
