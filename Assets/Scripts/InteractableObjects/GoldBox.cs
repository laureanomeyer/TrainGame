using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;


public class GoldBox : IInteractable
{
    private float currentGold;
    public float CurrentGold => currentGold;

    public GoldBox()
    {
        currentGold = 0;
        GameEvents.OnChangeGold += ChangeGoldInData;
    }

    public void Interact(IInventory playerRef) 
    {
        AddGold(playerRef.DepositGold());
    }

    public void AddGold(float amount) 
    {
        currentGold += amount;
        GameEvents.GoldBoxChanged(currentGold);
    }

    public void ChangeGoldInData()
    {
        GameManager.Instance.PlayerData.AddPlayerGold(currentGold);
    }

    public void OnDestroyObject() => GameEvents.OnChangeGold -= ChangeGoldInData;
}

