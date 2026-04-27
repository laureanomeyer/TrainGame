using System;

public static class GameEvents
{
    public static event Action<float> OnGoldEarned;
    
    public static event Action<float> OnGoldBoxChanged;

    public static event Action OnChangeGold;

    public static event Action OnChangeTrainData;

    public static event Action OnShoot;

    public static event Action OnShieldsBroken;

    public static event Action OnCoalEmpty;

    public static event Action OnWagonDestroyed;

    public static event Action OnEnemyDeath;

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

    public static void EnemyDeath()
    {
        OnEnemyDeath?.Invoke();
    }

}