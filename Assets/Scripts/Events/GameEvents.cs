using System;
using UnityEngine;

public class OnGoldEarnedEvent : IGameEvent 
{
    public float Amount; 

    public OnGoldEarnedEvent(float amount) 
    { 
        Amount = amount;
    }
}

public class OnGoldBoxChangedEvent : IGameEvent
{
    public float CurrentGold;

    public OnGoldBoxChangedEvent(float gold)
    {
        CurrentGold = gold;
    }
}

public class OnShootEvent : IGameEvent
{
    public float RateOfFire;

    public OnShootEvent(float rateOfFire)
    {
        RateOfFire = rateOfFire;
        AudioManager.Instance.Play($"RevolverShot{UnityEngine.Random.Range(3, 4)}");
    }
}

public class OnReloadEvent : IGameEvent
{
    public float ReloadTimer;

    public OnReloadEvent(float reloadTime)
    {
        ReloadTimer = reloadTime;
        AudioManager.Instance.Play($"RevolverMusket{2}");
    }
}

public class OnAmmoChangedEvent : IGameEvent
{
    public float Ammunition;

    public OnAmmoChangedEvent(float ammunition)
    {
        Ammunition = ammunition;
    }
}

public class OnShieldsBrokenEvent : IGameEvent
{
    public OnShieldsBrokenEvent()
    {
        AudioManager.Instance.Play("Projectile");
    }
}

public class OnCoalEmptyEvent : IGameEvent
{
    public OnCoalEmptyEvent(){}
}

public class OnWagonDestroyedEvent : IGameEvent
{
    public OnWagonDestroyedEvent() { }
}

public class OnEnemyDeathEvent : IGameEvent
{
    public UnityEngine.Vector3 Position;

    public OnEnemyDeathEvent(UnityEngine.Vector3 position)
    {
        Position = position;
    }
}

public class OnEnemyHitEvent : IGameEvent
{
    public UnityEngine.Vector3 Position;

    public OnEnemyHitEvent(UnityEngine.Vector3 position)
    {
        Position = position;
    }
}

public class OnActivateUiEvent : IGameEvent
{
    public bool Activated;

    public OnActivateUiEvent(bool activated)
    {
        Activated = activated;
    }
}

public class OnShowCursorEvent : IGameEvent
{
    public CursorType Cursor;

    public OnShowCursorEvent(CursorType cursor)
    {
        Cursor = cursor;
    }
}

public class OnShowGameplayCursorEvent : IGameEvent
{
    public bool Show;

    public OnShowGameplayCursorEvent(bool show)
    {
        Show = show;
    }
}

public class OnShowInteractEvent : IGameEvent
{
    public OnShowInteractEvent() { }
}

public class OnHideInteractEvent : IGameEvent
{
    public OnHideInteractEvent() { }
}

public class OnTakeFuelEvent : IGameEvent
{
    public OnTakeFuelEvent() { }
}

public class OnTakeGoldEvent : IGameEvent
{
    public OnTakeGoldEvent() { }
}

public class OnDropFuelEvent : IGameEvent
{
    public OnDropFuelEvent() { }
}

public class OnDropGoldEvent : IGameEvent
{
    public OnDropGoldEvent() { }
}

public class OnInteractPressedEvent : IGameEvent
{
    public OnInteractPressedEvent() { }
}

public class OnStatChangedEvent : IGameEvent
{
    public OnStatChangedEvent() { }
}