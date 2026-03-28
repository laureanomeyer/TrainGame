using System;
using UnityEngine;

public class WagonHP : IDamagable
{
    private float maxHp;
    private float currentHp;
    private float defense;
    private bool isBroken;
    private Action die;

    public bool IsBroken => isBroken;
    public float CurrentHp => currentHp;

    public WagonHP(float hp, float defense, Action deathAction)
    {
        this.maxHp = hp;
        currentHp = maxHp;
        this.defense = defense;
        isBroken = false;
        die = deathAction;
    }

    public void TakeDamage(float damageToTake) 
    {
        currentHp -= damageToTake * 100 / (100 + defense);

        if (currentHp < 0) 
        {
            Break();
        }

    }
    public void Repair(float deltaTime, float repairAmount) 
    {
        currentHp += repairAmount * deltaTime;
    }
    public void Break() 
    {
        isBroken = true;
    }
    public void BreakDown() 
    {
        die();
    }
}
