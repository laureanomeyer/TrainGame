using System;
using System.Numerics;
using UnityEngine;

public static class GameEvents
{
    public static event Action<float> OnGoldEarned;
    
    public static event Action<float> OnGoldBoxChanged;

    public static event Action OnChangeGold;

    public static event Action OnChangeTrainData;

    public static event Action OnShoot;

    public static event Action<float> OnAmmoChanged;

    public static event Action OnShieldsBroken;

    public static event Action OnCoalEmpty;

    public static event Action OnWagonDestroyed;

    public static event Action OnEnemyDeath;

    public static event Action OnShowInteract;

    public static event Action OnHideInteract;

    public static void GoldEarned(float amount)
    {
        OnGoldEarned?.Invoke(amount);
    }

    public static void GoldBoxChanged(float currentGold)
    {
        OnGoldBoxChanged?.Invoke(currentGold);
    }

    public static void ChangeGold()
    {
        OnChangeGold?.Invoke();
    }

    public static void ChangeTrainData()
    {
        OnChangeTrainData?.Invoke();
    }

    public static void ShootPerformed()
    {
        OnShoot?.Invoke();
    }
    public static void AmmoChanged(float currentAmmo)
    {
        OnAmmoChanged?.Invoke(currentAmmo);
    }

    public static void CoalEmpty()
    {
        OnCoalEmpty?.Invoke();
    }

    public static void ShieldsBroken()
    {
        OnShieldsBroken?.Invoke();
    }

    public static void WagonDestroyed()
    {
        OnWagonDestroyed?.Invoke();
    }

    public static void EnemyDeath(UnityEngine.Vector3 position)
    {
        OnEnemyDeath?.Invoke();
    }

    public static void ShowInteract()
    {
        OnShowInteract?.Invoke();
    }

    public static void HideInteract()
    {
        OnHideInteract?.Invoke();
    }
}