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
        HandleCoal(playerRef);
    }

    public void HandleCoal(IInventory playerRef)
    {
        if ((!hasCoal && !playerRef.HasCoal) || (hasCoal && playerRef.HasCoal)) return;

        else if(!hasCoal && playerRef.HasCoal)
        {
            hasCoal = true;
            playerRef.DepositCoal();
        }
        else if (hasCoal && !playerRef.HasCoal)
        {
            hasCoal = true;
            playerRef.CollectCoal();
        }


    }
}
