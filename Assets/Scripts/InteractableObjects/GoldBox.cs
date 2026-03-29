using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;


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
        Debug.Log(currentGold);
    }

    public void AddGold(float amount) 
    {
        currentGold += amount;
    }
}

