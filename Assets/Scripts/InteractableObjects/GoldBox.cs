using UnityEngine;

public class GoldBox : IInteractableWithInventory
{
    private bool canInteract = true;
    BoxCollider collider;
    private PlayerData playerDataRef;

    private float currentGold;
    public float CurrentGold => currentGold;

    public GoldBox(BoxCollider collider)
    {
        currentGold = 0;
        this.collider = collider;
        TutorialEvents.OnEnableGoldBox += SetCanInteract;
        playerDataRef = ServiceLocator.Get<PlayerData>();
    }

    public void Interact(IInventory playerRef)
    {
        if (!canInteract) return;

        AddGold(playerRef.DepositGold());

        EventBus.Publish(new OnDropGoldEvent());
    }

    public void AddGold(float amount)
    {
        if (amount <= 0) return;
        currentGold += amount;
        EventBus.Publish(new OnGoldBoxChangedEvent (currentGold));
        ChangeGoldInData(amount);

        if (GameManager.Instance.CurrentState == GameState.Tutorial)
            TutorialEvents.StartFuelUse();
    }

    public void ChangeGoldInData(float amount)
    {
        playerDataRef.AddPlayerGold(amount);
    }
    public void SetCanInteract(bool canInteract)
    {
        this.canInteract = canInteract;
        if (collider != null)
            collider.enabled = canInteract;
    }

    public void OnDestroyObject()
    {
        TutorialEvents.OnEnableGoldBox -= SetCanInteract;
    }
}

