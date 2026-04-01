using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;


public class LocomotiveFuel
{
    private float maxShield;
    private float currentShield;
    private float currentFuel;
    private float currentMaxFuel;
    private bool shieldTakenDamage;
    private float actualSpeed;
    private float fuelOptimizer;
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

        GameManager.Instance.TrainData.SetSpeed(actualSpeed);
    }
    public LocomotiveFuel(float shield, float maxFuel, float baseSpeed, float defense)
    {
        this.maxShield = shield;
        this.currentShield = shield;
        this.currentMaxFuel = maxFuel;
        currentFuel = maxFuel;
        this.actualSpeed = baseSpeed;
        fuelOptimizer = 1;
        fuelUseXSecond = actualSpeed / (2 * fuelOptimizer);
        this.defense = defense;
        shieldTakenDamage = false;
        fuelMaxCapacity = maxFuel;

        GameManager.Instance.TrainData.SetSpeed(actualSpeed);
    }

    public void Move(float deltaTime)
    {
        if (!hasFuel)
        {
            GameManager.Instance.TrainData.SetSpeed(0);
            return;
        }
        ConsumeFuel(fuelUseXSecond * deltaTime);

    }
    public void AddFuel(float amount)
    {
        currentFuel = Mathf.Clamp(currentFuel + amount, amount, currentMaxFuel);
        UpdateSharedSpeed();
    }

    public void RemoveFuel(float amount) //llamar a esta funcion x si hay alguien o algo que te reste nafta 
    {
        currentFuel = Mathf.Clamp(currentFuel - amount, 0f, currentMaxFuel);
        UpdateSharedSpeed();
        Debug.Log("Current Fuel: " + currentFuel);
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

    private void UpdateSharedSpeed()
    {
        if (hasFuel)
        {
            GameManager.Instance.TrainData.SetSpeed(actualSpeed);
        }
        else
        {
            GameManager.Instance.TrainData.SetSpeed(0);
        }
    }

    public void TakeDamage(float amount)
    {
        if (currentShield <= 0)
        {
            currentMaxFuel -= amount * 100 / (100 + defense);
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
            currentShield += 1 * deltaTime;
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