public class GoldBox : IInteractable
{
    private bool canInteract = true;

    private float currentGold;
    public float CurrentGold => currentGold;

    public GoldBox()
    {
        currentGold = 0;
        TutorialEvents.OnEnableGoldBox += SetCanInteract;
    }

    public void Interact(IInventory playerRef) 
    {
        if (!canInteract) return;

        AddGold(playerRef.DepositGold());

        GameEvents.DropGold();
    }

    public void AddGold(float amount) 
    {
        if (amount <= 0) return;
        currentGold += amount;
        GameEvents.GoldBoxChanged(currentGold);
        ChangeGoldInData(amount);

        if (GameManager.Instance.CurrentState == GameState.Tutorial)
            TutorialEvents.StartFuelUse();
    }

    public void ChangeGoldInData(float amount)
    {
        GameManager.Instance.Session.PlayerData.AddPlayerGold(amount);
    }
    public void SetCanInteract(bool canInteract)
    {
        this.canInteract = canInteract;
    }

    public void OnDestroyObject() 
    {
        TutorialEvents.OnEnableGoldBox += SetCanInteract;
    }
}

