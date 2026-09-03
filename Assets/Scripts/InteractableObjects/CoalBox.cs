
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
        Debug.Log("pipers");
        if ((!hasCoal && !playerRef.HasCoal) || (hasCoal && playerRef.HasCoal))
        {
            Debug.Log("piper2");
            return;
        } 
        else if(!hasCoal && playerRef.HasCoal)
        {
            Debug.Log("piper3");
            hasCoal = true;
            EventBus.Publish(new OnTakeCoalEvent());
        }
        else if (hasCoal && !playerRef.HasCoal)
        {
            Debug.Log("piper4");

            hasCoal = true;
            EventBus.Publish(new OnTakeCoalEvent());
        }
        else if (hasCoal && playerRef.HasCoal)
        {
            Debug.Log("piper5");
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
