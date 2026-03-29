public class PlayerInventory : IInventory
{
    private float goldAmount;
    private bool hasCoal;

    public float GoldAmount => goldAmount;
    public bool HasCoal => hasCoal;

    public PlayerInventory()
    {
        goldAmount = 100f;
        hasCoal = true;
    }

    public float DepositGold()
    {
        float goldToAdd = goldAmount;
        goldAmount = 0;
        return goldToAdd;
    }

    public bool DepositCoal()
    {
        hasCoal = !hasCoal;
        return hasCoal;
    }
}

