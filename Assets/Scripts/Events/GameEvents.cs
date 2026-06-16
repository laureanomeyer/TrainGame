using System;

public static class GameEvents
{
    public static event Action<float> OnGoldEarned;
    
    public static event Action<float> OnGoldBoxChanged;

    public static event Action OnChangeGold;

    public static event Action OnChangeTrainData;

    public static event Action<float> OnShoot;

    public static event Action<float> OnReloadStarted;

    public static event Action<float> OnAmmoChanged;

    public static event Action OnShieldsBroken;

    public static event Action OnCoalEmpty;

    public static event Action OnWagonDestroyed;

    public static event Action<UnityEngine.Vector3> OnEnemyDeath;

    public static event Action<UnityEngine.Vector3> OnEnemyHit;

    public static event Action<bool> OnActivateUi;

    public static event Action<bool> OnShowCursor;

    public static event Action OnShowInteract;

    public static event Action OnHideInteract;

    public static event Action OnTakeFuel;

    public static event Action OnTakeGold;

    public static event Action OnDropFuel;

    public static event Action OnDropGold;

    public static event Action OnInteractPressed;

    public static event Action OnStatChanged;

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

    public static void ShootPerformed(float rateOfFire)
    {
        OnShoot?.Invoke(rateOfFire);
    }
    public static void ReloadStarted(float reloadTimer)
    {
        OnReloadStarted?.Invoke(reloadTimer);
    }
    public static void AmmoChanged(float currentAmmo)
    {
        OnAmmoChanged?.Invoke(currentAmmo);
    }
    public static void ShowCursor(bool showCursor)
    {
        OnShowCursor?.Invoke(showCursor);
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
        OnEnemyDeath?.Invoke(position);
    }

    public static void EnemyHit(UnityEngine.Vector3 position)
    {
        OnEnemyHit?.Invoke(position);
    }
    public static void UiActivated(bool activated)
    {
        OnActivateUi?.Invoke(!activated);
    }

    public static void ShowInteract()
    {
        OnShowInteract?.Invoke();
    }

    public static void HideInteract()
    {
        OnHideInteract?.Invoke();
    }

    public static void TakeFuel()
    {
        OnTakeFuel?.Invoke();
    }

    public static void TakeGold()
    {
        OnTakeGold?.Invoke();
    }

    public static void DropFuel()
    {
        OnDropFuel?.Invoke();
    }

    public static void DropGold()
    {
        OnDropGold?.Invoke();
    }
    public static void InteractPressed()
    {
        OnInteractPressed?.Invoke();
    }

    public static void StatChanged()
    {
        OnStatChanged?.Invoke();
    }
}