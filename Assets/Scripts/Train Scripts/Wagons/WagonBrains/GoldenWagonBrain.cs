using UnityEngine;

public class GoldenWagonBrain : WagonBrain
{
    private GoldCollector collector;
    public GoldCollector Collector => collector;

    [SerializeField] private Material baseWagonMaterial;

    public override void Start()
    {
        base.Start();
        collector = new GoldCollector(hpController);
    }
    public override void Repair(float repairAmount)
    {
        if (hpController.IsBroken == true & hpController.CurrentHp > 0)
        {
            rendererWagon.material = baseWagonMaterial;
            hpController.IsBroken = false;
        }

        hpController.ReapirGoldenWagon(Time.deltaTime, repairAmount);
    }
}
