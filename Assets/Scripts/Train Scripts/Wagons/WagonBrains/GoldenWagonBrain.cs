using UnityEngine;

public class GoldenWagonBrain : WagonBrain
{
    private GoldCollector collector;
    public GoldCollector Collector => collector;

    [SerializeField] private Material baseWagonMaterial;

    new public void Start()
    {
        hp = RunManager.Instance.TrainCopyData.stats.trainMaxHp;
        defense = RunManager.Instance.TrainCopyData.stats.shields;
        hpController = new WagonHP(hp, defense, Break, canBreak);
        collector = new GoldCollector(hpController);
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
