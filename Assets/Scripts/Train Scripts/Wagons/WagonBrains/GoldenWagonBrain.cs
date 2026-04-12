using UnityEngine;

public class GoldenWagonBrain : WagonBrain
{
    private GoldCollector collector;
    public GoldCollector Collector => collector;

    new public void Start()
    {
        hp = RunManager.Instance.TrainCopyData.stats.trainMaxHp;
        defense = RunManager.Instance.TrainCopyData.stats.shields;
        hpController = new WagonHP(hp, defense, Break, canBreak);
        collector = new GoldCollector(hpController);
    }
}
