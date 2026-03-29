using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class CoalBox: IInteractable
{
    private bool hasCoal;
    public bool HasCoal => hasCoal;

    public CoalBox()
    {
        hasCoal = true;
    }

    public void Interact(IInventory playerRef)
    {      
        HandleCoal(playerRef.DepositCoal());
        Debug.Log(hasCoal);
    }

    public void HandleCoal(bool playerHasCoal)
    {
        hasCoal = !playerHasCoal;
    }
}
