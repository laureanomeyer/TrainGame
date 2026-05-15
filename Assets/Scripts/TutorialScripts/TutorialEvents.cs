using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;

public class TutorialEvents 
{
    public static Action<bool> OnSetMovementEnabled;
    public static Action<Vector3, List<IWagon>>OnSpawnEnemy; //Spawnea al enemigo
    public static Action<bool> OnSetAttackEnabled; //Permite al jugador disparar
    public static Action OnStartFuelUse; //Comienza a perder carbon
    public static Action<bool> OnSetCanConsume; //Activa y desactiva el carbon
    public static Action<bool> OnStartSpawningEnemies; //Comienza el spawn de enemigos reales
    public static Action<bool> OnSetTimerStarted; //Comienza el timer real

    public static Action<string> OnSetTutorialText;
    public static Action<bool> OnSetTutorialVisible;

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
    public static void SetMovementEnabled(bool can)
    {
        OnSetMovementEnabled?.Invoke(can);
    }
    public static void SetRunStarted(bool can)
    {
        OnStartSpawningEnemies?.Invoke(can);
    }
    public static void SetTimerStarted(bool can)
    {
        OnSetTimerStarted?.Invoke(can);
    }
    public static void SetAttackEnabled(bool can) 
    { 
        OnSetAttackEnabled?.Invoke(can);
    }
    public static void SetTutorialText(string text)
    {
        OnSetTutorialText?.Invoke(text);
    }
    public static void SetTutorialTextVisible(bool show)
    {
        OnSetTutorialVisible?.Invoke(show);
        OnSetTutorialVisible?.Invoke(show);
    }
}
