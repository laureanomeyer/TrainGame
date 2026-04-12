
using System.Diagnostics;

public class PlayerInventory : IInventory
{
    private float goldAmount;
    private bool hasCoal;

    public float GoldAmount { get => goldAmount; set => goldAmount = value; }
    public bool HasCoal => hasCoal;

    public PlayerInventory()
    {
        goldAmount = 100f;
        hasCoal = false;
    }

    public float DepositGold()
    {
        float goldToAdd = goldAmount;
        goldAmount = 0;
        return goldToAdd;
    }

    
     public void DepositCoal()
     {
        hasCoal = false;
    }

    public void CollectCoal()
    {
        hasCoal = true;
    }
}