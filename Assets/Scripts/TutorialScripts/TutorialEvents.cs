using System;
using UnityEngine;

public class TutorialEvents 
{
    public static Action OnSpawnEnemy;
    public static Action OnStartFuelUse;

    public static void SpawnEnemy()
    {
        OnSpawnEnemy?.Invoke();
    }
    public static void StartFuelUse()
    {
        OnStartFuelUse?.Invoke();
    }
}
