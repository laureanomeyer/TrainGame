using System;
using System.Collections.Generic;
using UnityEngine;

public class OnGoldEarnedEvent : IGameEvent 
{
    public float Amount; 

    public OnGoldEarnedEvent(float amount) 
    { 
        Amount = amount;
    }
}

public class OnCoalEarnedEvent : IGameEvent
{
    public float Amount;

    public OnCoalEarnedEvent(float amount)
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
        AudioManager.Instance.Play($"SFXDefaultShot");
    }
}

public class OnReloadEvent : IGameEvent
{
    public float ReloadTimer;

    public OnReloadEvent(float reloadTime)
    {
        ReloadTimer = reloadTime;
        AudioManager.Instance.Play($"SFXReloadMusket");
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

    public DropType DropType;

    public OnEnemyDeathEvent(UnityEngine.Vector3 position, DropType dropType)
    {
        Position = position;
        DropType = dropType;
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

    /// <summary>
    /// Poner falso para desactivar movimiento y CanAttack del jugador (por alguna razon)
    /// </summary>
    public OnActivateUiEvent(bool activated)
    {
        Activated = activated;
        Debug.Log(activated);
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

public class OnSpawnEnemyEvent : IGameEvent
{
    public Vector3 Position;
    public List<IWagon> List;

    public OnSpawnEnemyEvent(Vector3 position, List<IWagon> list) 
    {
        Position = position;
        List = list;
    }
}

public class OnSetAttackEnabledEvent : IGameEvent
{
    public bool Can;

    public OnSetAttackEnabledEvent(bool can)
    {
        Can = can;
    }
}

public class OnEnableCoalBoxEvent : IGameEvent
{
    public bool Enable;

    public OnEnableCoalBoxEvent(bool enable)
    {
        Enable = enable;
    }
}

public class OnEnableGoldBoxEvent : IGameEvent
{
    public bool Enable;

    public OnEnableGoldBoxEvent(bool enable)
    {
        Enable = enable;
    }
}

public class OnStartFuelUseEvent : IGameEvent
{
    public OnStartFuelUseEvent() { }
}

public class OnSetCanConsumeEvent : IGameEvent
{
    public bool Can;

    public OnSetCanConsumeEvent(bool can)
    {
        Can = can;
    }
}

public class OnStartSpawningEnemiesEvent : IGameEvent
{
    public bool Can;

    public OnStartSpawningEnemiesEvent(bool can)
    {
        Can = can;
    }
}

public class OnSetTimerStartedEvent : IGameEvent
{
    public bool Can;

    public OnSetTimerStartedEvent(bool can)
    {
        Can = can;
    }
}

public class OnSetTutorialTextEvent : IGameEvent
{
    public string Text;

    public OnSetTutorialTextEvent(string text)
    {
        Text = text;
    }
}

public class OnEnemyKilledEvent : IGameEvent
{
    public OnEnemyKilledEvent() { }
}

public class OnSetTutorialVisibleEvent : IGameEvent
{
    public bool Show;

    public OnSetTutorialVisibleEvent(bool show)
    {
        Show = show;
    }
}

public class OnRunEndedEvent : IGameEvent
{
    public RunResult Result;
    public OnRunEndedEvent(RunResult result) 
    {
        Result = result;
    }
}


#region Weapons Events

#region Wichester Events
public class OnUpdateEnemiesDamage : IGameEvent
{
    public List<Enemy> Enemies;

    public OnUpdateEnemiesDamage (List<Enemy> enemies)
    {
        this.Enemies = enemies;
    }
}

public class OnUpdateWinchesterLegadoLeftPoint : IGameEvent
{
    public int point;

    public OnUpdateWinchesterLegadoLeftPoint(int point)
    {
        this.point = point;
    }
}
public class OnUnlockWinchesterLegado : IGameEvent
{
    public OnUnlockWinchesterLegado()
    {
    }
}

public class OnWinchesterDetectedDeadEnemy : IGameEvent
{
    public OnWinchesterDetectedDeadEnemy()
    {
    }
}


#endregion

#region Spencer Events

public class OnSpencerDetectedDeadEnemy : IGameEvent
{
    public OnSpencerDetectedDeadEnemy()
    {
    }
}

public class OnUpdatedSpencerLegado : IGameEvent
{
    public OnUpdatedSpencerLegado()
    {
    }
}

public class OnUnlockSpencerLegado : IGameEvent
{
    public OnUnlockSpencerLegado()
    {
    }
}

#endregion

#region Coach Events

public class OnCoachDetectedDeadEnemy : IGameEvent
{
    public int point;
    public OnCoachDetectedDeadEnemy(int point)
    {
        this.point = point;
    }
}

public class OnUpdatedCoachLegado : IGameEvent
{
    public OnUpdatedCoachLegado()
    {
    }
}

public class OnUnlockCoachLegado : IGameEvent
{
    public OnUnlockCoachLegado()
    {
    }
}

#endregion

#endregion