
using UnityEngine;

public class GoldBox : IInteractable
{
    private float currentGold;
    public float CurrentGold => currentGold;

    public GoldBox()
    {
        currentGold = 0;
    }

    public void Interact(IInventory playerRef) 
    {
        AddGold(playerRef.DepositGold());
        GameEvents.DropGold();
        TutorialEvents.StartFuelUse();
    }

    public void AddGold(float amount) 
    {
        currentGold += amount;
        GameEvents.GoldBoxChanged(currentGold);
        ChangeGoldInData(amount);
    }

    public void ChangeGoldInData(float amount)
    {
        GameManager.Instance.Session.PlayerData.AddPlayerGold(amount);
    }

    public void OnDestroyObject() { }
}

