using System.Collections.Generic;
using System;
using UnityEngine;

public class TutorialEvents 
{
    public static Action<Vector3, List<IWagon>>OnSpawnEnemy;
    public static Action OnStartFuelUse;
    public static Action<bool> OnSetCanConsume;
    public static Action<bool> OnSetRunStarted;
    public static Action<bool> OnSetTimerStarted;
    public static Action<bool> OnSetAttackEnabled;

    public static void SpawnEnemy(Vector3 pos, List<IWagon> list)
    {
        OnSpawnEnemy?.Invoke(pos, list);
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
    public static void SetAttackEnabled(bool can) 
    { 
        OnSetAttackEnabled?.Invoke(can);
    }
}
