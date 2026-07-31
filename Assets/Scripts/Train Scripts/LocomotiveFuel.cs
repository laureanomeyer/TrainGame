using System;
using UnityEngine;

public class LocomotiveFuel
{
    public event Action OnDestroyed;

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
    private bool canConsume = true;
    private bool destroyed;

    private float fuelCapacity;
    private float fuelMaxCapacity;
    private TrainData trainDataRef;

    private bool hasFuel => currentFuel > 0f;
    public float CurrentFuel => currentFuel;
    public float CurrentMaxFuel => currentMaxFuel;

    public float CurrentShield => currentShield;
    public float MaxShield => maxShield;

    public float FuelCapacity => fuelCapacity;
    public float FuelMaxCapaciy => fuelMaxCapacity;

    public bool IsDestroyed => destroyed;

    public LocomotiveFuel(float shield, float maxFuel, float defense, float fuelOptimizer)
    {
        this.maxShield = shield;
        this.currentShield = shield;
        this.currentMaxFuel = maxFuel;
        this.actualSpeed = 100f;
        this.fuelOptimizer = fuelOptimizer;
        this.defense = defense;

        currentFuel = maxFuel;
        fuelUseXSecond = 1 / (2 * fuelOptimizer);
        shieldTakenDamage = false;
        fuelMaxCapacity = maxFuel;

        EventBus.Subscribe<OnSetCanConsumeEvent>(SetCanConsume);

        trainDataRef = ServiceLocator.Get<TrainData>();
        trainDataRef.SetSpeed(actualSpeed);

        canConsume = !GameManager.Instance.IsTutorial;
    }

    public void Destroy()
    {
        EventBus.Unsubscribe<OnSetCanConsumeEvent>(SetCanConsume);
        OnDestroyed = null;
    }

    public void Move(float deltaTime)
    {
        if (destroyed) return;
        if (!GameManager.Instance.IsGameplayState) return;

        if (!hasFuel)
        {
            trainDataRef.SetSpeed(0);
            return;
        }

        if (canConsume) ConsumeFuel(fuelUseXSecond * deltaTime);
    }

    public void AddFuel()
    {
        if (destroyed) return;

        currentFuel = currentMaxFuel;
        AudioManager.Instance.Play("VaultOpening");
        UpdateSharedSpeed();
    }

    public void RemoveFuel(float amount) //llamar a esta funcion x si hay alguien o algo que te reste nafta
    {
        if (destroyed) return;

        currentFuel = Mathf.Clamp(currentFuel - amount, 0f, currentMaxFuel);
        UpdateSharedSpeed();
    }

    private void ConsumeFuel(float amount) //gasto natural del tren
    {
        currentFuel = Mathf.Clamp(currentFuel - amount, 0f, currentMaxFuel);
        if (currentFuel <= 0) EventBus.Publish(new OnCoalEmptyEvent());
        UpdateSharedSpeed();
    }

    public void ModifyOptimizer(float wagonEffect)
    {
        fuelOptimizer = fuelOptimizer + wagonEffect;
        fuelUseXSecond = 1 / (2 * fuelOptimizer);
    }

    private void UpdateSharedSpeed()
    {
        if (hasFuel)
        {
            trainDataRef.SetSpeed(actualSpeed);
            return;
        }

        trainDataRef.SetSpeed(0);
        RaiseDestroyed();
    }

    public void TakeDamage(float amount)
    {
        if (destroyed) return;
        if (!GameManager.Instance.IsGameplayState) return;

        if (currentShield <= 0)
        {
            currentMaxFuel -= amount / defense;
            currentFuel = Mathf.Clamp(currentFuel, 0, Mathf.Max(currentMaxFuel, 0f));

            if (currentMaxFuel <= 0)
                RaiseDestroyed();
        }
        else
        {
            currentShield -= amount / defense;
            timer = 0;
            shieldTakenDamage = true;
            if (currentShield <= 0) EventBus.Publish(new OnShieldsBrokenEvent());
        }
    }

    public void UpdateShield(float deltaTime)
    {
        if (destroyed) return;
        if (!GameManager.Instance.IsGameplayState) return;

        if (!shieldTakenDamage)
        {
            currentShield = Mathf.Clamp(currentShield + 5 * deltaTime, 0f, maxShield);
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

    private void RaiseDestroyed()
    {
        if (destroyed) return;

        destroyed = true;
        OnDestroyed?.Invoke();
    }

    public void SetCanConsume(OnSetCanConsumeEvent canConsumeEvent)
    {
        this.canConsume = canConsumeEvent.Can;
    }
}