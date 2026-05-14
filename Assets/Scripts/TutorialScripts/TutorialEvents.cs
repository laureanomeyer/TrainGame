using System;
using UnityEngine;

public class TutorialEvents 
{
    public static Action OnSpawnEnemy;
    public static Action OnStartFuelUse;
    public static Action<bool> OnSetCanConsume;
    public static Action<bool> OnSetRunStarted;
    public static Action<bool> OnSetTimerStarted;

    public static void SpawnEnemy()
    {
        OnSpawnEnemy?.Invoke();
    }
    public static void StartFuelUse()
    {
        OnStartFuelUse?.Invoke();
    }
    public static void SetCanConsume(bool can)
    {
        OnSetCanConsume?.Invoke(can);
    }
    public static void SetRunStarted(bool can)
    {
        OnSetRunStarted?.Invoke(can);
    }
    public static void SetTimerStarted(bool can)
    {
        OnSetTimerStarted?.Invoke(can);
    }
}
