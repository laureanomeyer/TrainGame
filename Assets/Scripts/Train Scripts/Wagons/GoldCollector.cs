
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

    public void CollectGold(int amount)
    {
        if (wagonHP.IsBroken == false)
        {
            gold += amount;
        }
        else
        {
            EmptyGold();
        }
    }

    public float GiveGold()
    {
        float goldToGive = gold;
        EmptyGold();

        return goldToGive;
    }

    public void EmptyGold()
    {
        gold = 0;
    }
}
