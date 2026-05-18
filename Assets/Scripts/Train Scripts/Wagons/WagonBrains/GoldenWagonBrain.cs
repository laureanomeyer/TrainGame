using TMPro;
using UnityEngine;

public class GoldenWagonBrain : WagonBrain
{
    private GoldCollector collector;
    public GoldCollector Collector => collector;

    [Header("Golden wagon data")]
    [SerializeField] private TextMeshProUGUI currentGoldUI;
    [SerializeField] private Transform goldBox;
    [SerializeField] private Material baseWagonMaterial;

    public override void Start()
    {
        base.Start();
        collector = new (hpController, currentGoldUI);
        GameManager.Instance.Session.TrainData.SetGoldBox(goldBox);
        floorRenderWagon.material = baseWagonMaterial;
    }

    public override void Repair(float repairAmount)
    {
        if (hpController.IsBroken == true & hpController.CurrentHp > 0)
        {
            floorRenderWagon.material = baseWagonMaterial;
            hpController.IsBroken = false;
        }

        hpController.Repair(repairAmount * Time.deltaTime);

        if (hpWorldUI != null)
        {
            hpWorldUI.UpdateHp(hpController.CurrentHp, hpController.MaxHp);
        }
    }
    public override void TakeDamage(float damageAmount)
    {
        base.TakeDamage(damageAmount);
        if (hpController.CurrentHp <= 0 && hpController != null)
        {
            collector.EmptyGold();
        }

    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        collector.ActivateOnDestroy();
    }
}
