using System;
using UnityEngine;

public class WagonHP : IDamagable
{
    private float maxHp;
    private float currentHp;
    private float defense;
    private bool isBroken;
    private bool canBreak;

    private Action die;

    public bool IsBroken { get => isBroken; set => isBroken = value; }
    public float CurrentHp => currentHp;

    public WagonHP(float hp, float defense, Action deathAction, bool canBreak)
    {
        this.maxHp = hp;
        currentHp = maxHp;
        this.defense = defense;
        isBroken = false;
        die = deathAction;
        this.canBreak = canBreak;
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
        if (!IsBroken)
        {
            currentHp += repairAmount * deltaTime;
        }
    }
    public void Break() 
    {
        isBroken = true;

        BreakDown();
    }
    public void BreakDown() 
    {
        die();
    }
}
