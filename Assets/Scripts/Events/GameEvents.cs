using System;

public static class GameEvents
{
    public static event Action<int> OnGoldEarned;

    public static void GoldEarned(int amount)
    {
        OnGoldEarned?.Invoke(amount);
    }
}