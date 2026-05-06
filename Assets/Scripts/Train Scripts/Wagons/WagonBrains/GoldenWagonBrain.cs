using TMPro;
using UnityEngine;

public class GoldenWagonBrain : WagonBrain
{
    private GoldCollector collector;
    public GoldCollector Collector => collector;

    [SerializeField] private TextMeshProUGUI currentGoldUI;
    [SerializeField] private Transform goldBox;
    [SerializeField] private Material baseWagonMaterial;

    public override void Start()
    {
        base.Start();
        collector = new GoldCollector(hpController, currentGoldUI);
        GameManager.Instance.Session.TrainData.SetGoldBox(goldBox);
    }

    private void Update()
    {
        if (hpController.CurrentHp <= 0)
        {
            collector.EmptyGold();
        }
        else
        {
            rendererWagon.material = baseWagonMaterial;
        }
    }

    public override void Repair(float repairAmount)
    {
        if (hpController.IsBroken == true & hpController.CurrentHp > 0)
        {
            rendererWagon.material = baseWagonMaterial;
            hpController.IsBroken = false;
        }

        hpController.ReapirGoldenWagon(Time.deltaTime, repairAmount);

        if (hpWorldUI != null)
        {
            hpWorldUI.UpdateHp(hpController.CurrentHp, hpController.MaxHp);
        }
    }
}
