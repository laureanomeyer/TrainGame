using UnityEngine;

public class GoldCollector : MonoBehaviour
{
    private WagonHP wagonHP;

    private int gold;
    public int Gold => gold;

    private void Start()
    {
        wagonHP = GetComponent<WagonBrain>().HPController;
        GameEvents.OnGoldEarned += CollectGold;
    }
    void OnDestroy() => GameEvents.OnGoldEarned -= CollectGold;

    public void CollectGold(int amount)
    {
        if (wagonHP.IsBroken)
        {
            return;
        }
        gold += amount;
    }

    public int GiveGold()
    {
        int goldToGive = gold;
        EmptyGold();

        return goldToGive;
    }

    public void EmptyGold()
    {
        gold = 0;
    }
}
