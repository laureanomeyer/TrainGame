
using System.Diagnostics;

public class GoldCollector
{
    private WagonHP wagonHP;

    private float gold;
    public float Gold => gold;

    public GoldCollector(WagonHP hpController)
    {
        wagonHP = hpController;
        GameEvents.OnGoldEarned += CollectGold;
    }
    void OnDestroy() => GameEvents.OnGoldEarned -= CollectGold;

    public void CollectGold(float amount)
    {
        if (wagonHP.CurrentHp > 0)
        {
            gold += amount * RunManager.Instance.TrainCopyData.LocomotiveStatsMultiplicator.goldBonus;
        }
        else
        {
            EmptyGold();
        }
    }

    public float GiveGold()
    {
        if (wagonHP.CurrentHp > 0)
        {
            float goldToGive = gold;
            EmptyGold();

            return goldToGive;
        }
        else
        {
            return 0;
        }
    }

    public void EmptyGold()
    {
        gold = 0;
    }
}
