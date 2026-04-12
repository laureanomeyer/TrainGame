using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public struct TrainStats
{
    public float fuelOptimizer;
    public float trainMaxHp;
    public float shields;
    public float goldBonus;
    public float damageBonus;
    public float attackSpeed;
    public float baseSpeed;
    public TrainStats(float trainMaxHp, float shields, float goldBonus, float damageBonus, float attackSpeed, float fuelOptimizer, float baseSpeed)
    { 
        this.trainMaxHp = trainMaxHp;
        this.shields = shields;
        this.goldBonus = goldBonus;
        this.damageBonus = damageBonus;
        this.attackSpeed = attackSpeed;
        this.fuelOptimizer = fuelOptimizer;
        this.baseSpeed = baseSpeed;
    }
    public TrainStats(LocomotiveStats trainData)
    {
        trainMaxHp = trainData.maxHp;
        shields = trainData.defense;
        goldBonus = trainData.goldMultyplier;
        damageBonus = trainData.damageMultyplier;
        attackSpeed = trainData.attackSpeed;
        fuelOptimizer = trainData.fuelOptimizer;
        baseSpeed = trainData.baseSpeed;
    }


    //Funcion encargada de sumar las varibles de dos TrainsStats
    public static TrainStats operator +(TrainStats x, TrainStats y)
    {
        return new TrainStats
        {
            fuelOptimizer = x.fuelOptimizer + y.fuelOptimizer,
            trainMaxHp = x.trainMaxHp + y.trainMaxHp,
            shields = x.shields + y.shields,
            goldBonus = x.goldBonus + y.goldBonus,
            damageBonus = x.damageBonus + y.damageBonus,
            attackSpeed = x.attackSpeed + y.attackSpeed,
            baseSpeed = x.baseSpeed + y.baseSpeed
        };
    }
}



public class LocomotiveFuel
{
    private float maxShield;
    private float currentShield;
    private float currentFuel;
    private float currentMaxFuel;
    private bool shieldTakenDamage;
    private float actualSpeed;
    public float fuelOptimizer;
    private float fuelUseXSecond;
    private float defense;
    private float timer = 0;

    private float fuelCapacity;
    private float fuelMaxCapacity;

    private bool hasFuel => currentFuel > 0f;
    public float CurrentFuel => currentFuel;
    public float CurrentMaxFuel => currentMaxFuel;

    public float CurrentShield => currentShield;
    public float MaxShield => maxShield;    

    public float FuelCapacity => fuelCapacity;
    public float FuelMaxCapaciy => fuelMaxCapacity;

    public LocomotiveFuel(float shield, float maxFuel, float baseSpeed, float defense, float fuelOptimizer)
    {
        this.maxShield = shield;
        this.currentShield = shield;
        this.currentMaxFuel = maxFuel;
        currentFuel = maxFuel;
        this.actualSpeed = baseSpeed;
        this.fuelOptimizer = fuelOptimizer;
        fuelUseXSecond = actualSpeed / (2 * fuelOptimizer);
        this.defense = defense;
        shieldTakenDamage = false;
        fuelMaxCapacity = maxFuel;

        RunManager.Instance.TrainCopyData.SetSpeed(actualSpeed);
    }

    public void Move(float deltaTime)
    {
        if (!hasFuel)
        {
            RunManager.Instance.TrainCopyData.SetSpeed(0);
            return;
        }
        ConsumeFuel(fuelUseXSecond * deltaTime);


    }
    public void AddFuel()
    {
        currentFuel = currentMaxFuel;
        UpdateSharedSpeed();
    }

    public void RemoveFuel(float amount) //llamar a esta funcion x si hay alguien o algo que te reste nafta 
    {
        currentFuel = Mathf.Clamp(currentFuel - amount, 0f, currentMaxFuel);
        UpdateSharedSpeed();
    }

    private void ConsumeFuel(float amount) //gasto natural del tren
    {
        currentFuel = Mathf.Clamp(currentFuel - amount, 0f, currentMaxFuel);
        UpdateSharedSpeed();
    }

    private void ModifySpeed(float speedToAdd)
    {
        actualSpeed += speedToAdd;
        fuelUseXSecond = actualSpeed / (2 * fuelOptimizer);
        UpdateSharedSpeed();
    }
    public void ModifyOptimizer(float wagonEffect)
    {
        fuelOptimizer = fuelOptimizer + wagonEffect;
        fuelUseXSecond = actualSpeed / (2 * fuelOptimizer);
        Debug.Log(fuelOptimizer);
    }

    private void UpdateSharedSpeed()
    {
        if (hasFuel)
        {
            RunManager.Instance.TrainCopyData.SetSpeed(actualSpeed);
        }
        else
        {
            RunManager .Instance.TrainCopyData.SetSpeed(0);
            GameManager.Instance.ResetScene();
        }
    }

    public void TakeDamage(float amount)
    {
        if (currentShield <= 0)
        {
            currentMaxFuel -= amount / defense;

            if (currentMaxFuel <= 0)
            {
                GameManager.Instance.ResetScene();
            }

            currentFuel = Mathf.Clamp(currentFuel, 0, currentMaxFuel);
        }
        else
        {
            currentShield -= amount * 100 / (100 + defense);
            timer = 0;
            shieldTakenDamage = true;
        }
    }

    public void UpdateShield(float deltaTime)
    {
        if (!shieldTakenDamage) 
        {
            currentShield += 5 * deltaTime;
            Mathf.Clamp(currentShield, 0, maxShield);

            if (currentShield >= maxShield)
            {
                currentShield = maxShield;
            }
        }

        else
        {           
            timer += deltaTime;

            if (timer >= 3) 
            { 
                shieldTakenDamage = false;
                timer = 0;
            }
        }

    }
}