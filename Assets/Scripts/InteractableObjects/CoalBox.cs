
using UnityEngine;

public class CoalBox: IInteractableWithInventory
{
    private bool canInteract = true;

    private float charges;
    private bool hasCoal;
    public bool HasCoal => hasCoal;
    public float Charges => charges;

    public CoalBox()
    {
        charges = 1f;
        hasCoal = true;

        TutorialEvents.OnEnableCoalBox += SetCanInteract;
    }
    
    public void Interact(IInventory playerRef)
    {
        if (canInteract)
            HandleCoal(playerRef);  
    }

    public void HandleCoal(IInventory playerRef)
    {
        if ((!hasCoal && !playerRef.HasCoal) || (hasCoal && playerRef.HasCoal)) return;

        else if(!hasCoal && playerRef.HasCoal)
        {
            hasCoal = true;
            playerRef.DepositCoal();
            GameEvents.TakeFuel();
        }
        else if (hasCoal && !playerRef.HasCoal)
        {
            hasCoal = true;
            playerRef.CollectCoal();
            GameEvents.TakeFuel();
        }
    }

    public void NewHandleCoal(IInventory playerRef)
    {
        if ((!hasCoal && !playerRef.HasCoal) || (hasCoal && playerRef.HasCoal)) return;

        charges -= 1;
        playerRef.CollectCoal();
        hasCoal = (charges > 0);
        GameEvents.TakeFuel();    
    }

    public void AddCharges(float amount)
    {
        charges += amount;
    }
    public void TakeCharges(float amount)
    {
        charges -= amount;
    }
    public void SetCharges(float amount)
    {
        charges = amount;
    }
    public void SetCanInteract(bool canInteract)
    {
        this.canInteract = canInteract;
    }

    public void OnDestroyObject()
    {
        TutorialEvents.OnEnableCoalBox -= SetCanInteract;
    }
}
