using UnityEngine;

public class GoldCollector : MonoBehaviour
{
    private int gold;
    public int Gold => gold;

    public void CollectGold(int amount)
    {
        gold += amount;
    }

    public int GiveGold()
    {
        int goldToGive = gold;
        gold = 0;

        return goldToGive;
    }
}
