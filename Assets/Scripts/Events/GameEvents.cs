using System;

public static class GameEvents
{
    public static event Action<float> OnGoldEarned;
    
    public static event Action<float> OnGoldBoxChanged;

    public static event Action OnChangeGold;

    public static event Action OnChangeTrainData;

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

}