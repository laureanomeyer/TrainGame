using UnityEngine;

public class GoldenWagonBrain : WagonBrain
{
    private GoldCollector collector;
    public GoldCollector Collector => collector;

    [SerializeField] private Material baseWagonMaterial;

    new public void Start()
    {
        hp = RunManager.Instance.TrainCopyData.LocomotiveStatsMultiplicator.trainMaxHp;
        defense = RunManager.Instance.TrainCopyData.LocomotiveStatsMultiplicator.shields;

        SetUpWagonHP();

        collector = new GoldCollector(hpController);


        //Debug.Log("Base HP SO: " + SM*(RunManager.Instance.TrainCopyData.LocomotiveStatsMultiplicator.trainMaxHp * RunManager.Instance.TrainCopyData.LocomotiveStatsMultiplicator.shields));
    }

    public override void Repair(float repairAmount)
    {
        if (hpController.IsBroken == true & hpController.CurrentHp > 0)
        {
            rendererWagon.material = baseWagonMaterial;
            hpController.IsBroken = false;
        }

        hpController.Repair(Time.deltaTime, repairAmount);
    }
}
