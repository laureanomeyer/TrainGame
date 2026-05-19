using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class CoalBox: IInteractable
{
    private float charges;
    private bool hasCoal;
    public bool HasCoal => hasCoal;
    public float Charges => charges;

    public CoalBox()
    {
        charges = 1f;
        hasCoal = true;
    }
    
    public void Interact(IInventory playerRef)
    {      
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

    public void OnDestroyObject()
    {

    }
}
