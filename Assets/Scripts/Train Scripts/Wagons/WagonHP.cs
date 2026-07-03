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
    public float MaxHp => maxHp;

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
        currentHp -= damageToTake / defense;

        if (currentHp < 0) 
        {
            BreakDown();
        }

    }
    public void Repair(float repairAmount, float delta) 
    {
        if(IsBroken && canBreak) return;

        currentHp += repairAmount * delta;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);

    }
    public void BreakDown() 
    {
        die();

        IsBroken = true;
    }
    public void OnMaxHpChanged(float newMaxHp)
    {
        float ratio = currentHp / maxHp; 
        maxHp = newMaxHp;
        currentHp = maxHp * ratio;
    }

    public void forceHp(float amount)
    {
        currentHp = amount;
    }
}
