using TMPro;

public class GoldCollector
{
    private WagonHP wagonHP;

    private float gold;
    public float Gold => gold;

    private TextMeshProUGUI goldDisplayUI;

    public GoldCollector(WagonHP hpController, TextMeshProUGUI CurrentGoldUI)
    {
        wagonHP = hpController;
        goldDisplayUI = CurrentGoldUI;
        GameEvents.OnGoldEarned += CollectGold;
    }
    void OnDestroy() => GameEvents.OnGoldEarned -= CollectGold;

    public void CollectGold(float amount)
    {
        if (wagonHP.CurrentHp > 0)
        {
            gold += amount * GameManager.Instance.Session.StatSystem.GetStat(StatType.GoldMultiplier);
            goldDisplayUI.text = "$" + gold;
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
        goldDisplayUI.text = string.Empty;
    }
}
