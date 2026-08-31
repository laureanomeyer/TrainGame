
using UnityEngine;

public class CoalBox: IInteractableWithInventory
{
    private bool canInteract = true;
    private BoxCollider collider;



    private float charges;
    private bool hasCoal;

    public bool HasCoal => hasCoal;
    public float Charges => charges;

    public CoalBox(BoxCollider collider)
    {
        charges = 2;
        hasCoal = true;
        this.collider = collider;
        EventBus.Subscribe<OnEnableCoalBoxEvent>(SetCanInteract);
    }

    public void OnDestroyObject()
    {
        EventBus.Unsubscribe<OnEnableCoalBoxEvent>(SetCanInteract);
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
            EventBus.Publish(new OnTakeCoalEvent());
        }
        else if (hasCoal && !playerRef.HasCoal)
        {
            hasCoal = true;
            playerRef.CollectCoal();
            EventBus.Publish(new OnTakeCoalEvent());
        }
        else if (hasCoal && playerRef.HasCoal)
        {
            Debug.Log("hjsbadkldgb lwuy");
            return;
        }
    }

    public void NewHandleCoal(IInventory playerRef)
    {
        if ((!hasCoal && !playerRef.HasCoal) || (hasCoal && playerRef.HasCoal)) return;

        charges -= 1;
        playerRef.CollectCoal();
        hasCoal = (charges > 0);
        EventBus.Publish(new OnTakeFuelEvent());    
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
    public void SetCanInteract(OnEnableCoalBoxEvent canInteractEvent)
    {
        this.canInteract = canInteractEvent.Enable;
        collider.enabled = canInteractEvent.Enable;
    }

    
}
