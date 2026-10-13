using System;
using System.Collections.Generic;
using UnityEngine;

public readonly struct OnGoldEarnedEvent : IGameEvent
{
    public readonly float Amount;

    public OnGoldEarnedEvent(float amount)
    {
        Amount = amount;
    }
}

public readonly struct OnCoalEarnedEvent : IGameEvent
{
    public readonly float Amount;
    public readonly bool Set;

    public OnCoalEarnedEvent(float amount, bool Set = false)
    {
        Amount = amount;
        this.Set = Set;
    }
}

public readonly struct OnGoldBoxChangedEvent : IGameEvent
{
    public readonly float CurrentGold;

    public OnGoldBoxChangedEvent(float gold)
    {
        CurrentGold = gold;
    }
}

public readonly struct OnShootEvent : IGameEvent
{
    public readonly float RateOfFire;
    public readonly float ShotStrenght;

    public OnShootEvent(float rateOfFire, float shotStrenght = 1)
    {
        RateOfFire = rateOfFire;
        ShotStrenght = shotStrenght;
    }
}

public readonly struct OnReloadEvent : IGameEvent
{
    public readonly float ReloadTimer;

    public OnReloadEvent(float reloadTime)
    {
        ReloadTimer = reloadTime;
        //AudioManager.Instance.Play($"SFXReloadMusket");
    }
}

public readonly struct OnAmmoChangedEvent : IGameEvent
{
    public readonly float Ammunition;

    public OnAmmoChangedEvent(float ammunition)
    {
        Ammunition = ammunition;
    }
}

public readonly struct OnShieldsBrokenEvent : IGameEvent { }

public readonly struct OnCoalEmptyEvent : IGameEvent { }

public readonly struct OnWagonDestroyedEvent : IGameEvent
{
    public readonly IWagon WagonInstance;

    public OnWagonDestroyedEvent(IWagon wagonInstance)
    {
        WagonInstance = wagonInstance;
    }
}

public readonly struct OnEnemyDeathEvent : IGameEvent
{
    public readonly UnityEngine.Vector3 Position;
    public readonly DropType DropType;
    public readonly float DropAmount;

    public OnEnemyDeathEvent(UnityEngine.Vector3 position, DropType dropType, float dropAmount)
    {
        Position = position;
        DropType = dropType;
        DropAmount = dropAmount;
    }
}

public readonly struct OnEnemyBecameInactiveEvent : IGameEvent
{
    public readonly Enemy Enemy;

    public OnEnemyBecameInactiveEvent(Enemy enemy)
    {
        Enemy = enemy;
    }
}

public readonly struct OnArcMotionEnded : IGameEvent
{
    public readonly ArcMotion ArcMotion;

    public OnArcMotionEnded(ArcMotion arcMotion)
    {
        ArcMotion = arcMotion;
    }
}

public readonly struct OnGoldenWagonIsRepairedEvent : IGameEvent
{
    public readonly IWagon goldenWagonInstance;

    public OnGoldenWagonIsRepairedEvent(IWagon goldenWagonInstance)
    {
        this.goldenWagonInstance = goldenWagonInstance;
    }
}

public readonly struct OnEnemyHitEvent : IGameEvent
{
    public readonly UnityEngine.Vector3 Position;

    public OnEnemyHitEvent(UnityEngine.Vector3 position)
    {
        Position = position;
    }
}

public readonly struct OnActivateUiEvent : IGameEvent
{
    public readonly bool Activated;

    /// <summary>
    /// Poner falso para desactivar movimiento y CanAttack del jugador (por alguna razon)
    /// </summary>
    public OnActivateUiEvent(bool activated)
    {
        Activated = activated;
    }
}

public readonly struct OnFreezePlayerEvent : IGameEvent
{
    public readonly bool Activated;

    /// <summary>
    /// Poner falso para desactivar movimiento y CanAttack del jugador (por alguna razon)
    /// </summary>
    public OnFreezePlayerEvent(bool activated)
    {
        Activated = activated;
    }
}

public readonly struct OnActivateNonPausableUI : IGameEvent
{
    public readonly bool Activated;

    /// <summary>
    /// Poner falso para desactivar movimiento y CanAttack del jugador (por alguna razon)
    /// </summary>
    public OnActivateNonPausableUI(bool activated)
    {
        Activated = activated;
    }
}

/// <summary>
/// Fuerza cerrar todas las Ui correctamente
/// </summary>
public readonly struct OnForceCloseAllUI : IGameEvent { }

public readonly struct OnShowCursorEvent : IGameEvent
{
    public readonly CursorType Cursor;

    public OnShowCursorEvent(CursorType cursor)
    {
        Cursor = cursor;
    }
}

public readonly struct OnShowGameplayCursorEvent : IGameEvent
{
    public readonly bool Show;

    public OnShowGameplayCursorEvent(bool show)
    {
        Show = show;
    }
}

public readonly struct OnShowInteractEvent : IGameEvent { }

public readonly struct OnHideInteractEvent : IGameEvent { }

public readonly struct OnTakeFuelEvent : IGameEvent { }

public readonly struct OnTakeGoldEvent : IGameEvent { }

public readonly struct OnTakeCoalEvent : IGameEvent { }

public readonly struct OnSetCoalWaypoint : IGameEvent
{
    public readonly bool Show;

    public OnSetCoalWaypoint(bool show)
    {
        Show = show;
    }
}

public readonly struct OnDeactivateFuelChargerWaypoint : IGameEvent { }

public readonly struct OnSetGoldWaypoint : IGameEvent
{
    public readonly bool Show;

    public OnSetGoldWaypoint(bool show)
    {
        Show = show;
    }
}

public readonly struct OnSetGoldWagonWaypoint : IGameEvent
{
    public readonly bool Show;

    public OnSetGoldWagonWaypoint(bool show)
    {
        Show = show;
    }
}

public readonly struct OnDropFuelEvent : IGameEvent { }

public readonly struct OnDropGoldEvent : IGameEvent { }

public readonly struct OnInteractPressedEvent : IGameEvent { }

public readonly struct OnStatChangedEvent : IGameEvent { }

public readonly struct OnSpawnEnemyEvent : IGameEvent
{
    public readonly Vector3 Position;
    public readonly List<IWagon> List; // En el original nunca se asignaba (siempre null). Si no lo usas en ningun lado, borralo.
    public readonly EnemyData Enemy;

    public OnSpawnEnemyEvent(Vector3 position, EnemyData enemy)
    {
        Position = position;
        List = null; // los structs obligan a asignar todos los campos en el constructor
        Enemy = enemy;
    }
}

public readonly struct OnSetAttackEnabledEvent : IGameEvent
{
    public readonly bool Can;

    public OnSetAttackEnabledEvent(bool can)
    {
        Can = can;
    }
}

public readonly struct OnEnableCoalBoxEvent : IGameEvent
{
    public readonly bool Enable;

    public OnEnableCoalBoxEvent(bool enable)
    {
        Enable = enable;
    }
}

public readonly struct OnEnableGoldBoxEvent : IGameEvent
{
    public readonly bool Enable;

    public OnEnableGoldBoxEvent(bool enable)
    {
        Enable = enable;
    }
}

public readonly struct OnStartFuelUseEvent : IGameEvent { }

public readonly struct OnSetCanConsumeEvent : IGameEvent
{
    public readonly bool Can;

    public OnSetCanConsumeEvent(bool can)
    {
        Can = can;
    }
}

public readonly struct OnStartSpawningEnemiesEvent : IGameEvent
{
    public readonly bool Can;

    public OnStartSpawningEnemiesEvent(bool can)
    {
        Can = can;
    }
}

public readonly struct OnSetTimerStartedEvent : IGameEvent
{
    public readonly bool Can;

    public OnSetTimerStartedEvent(bool can)
    {
        Can = can;
    }
}

public readonly struct OnSetTutorialTextEvent : IGameEvent
{
    public readonly string Text;

    public OnSetTutorialTextEvent(string text)
    {
        Text = text;
    }
}

public readonly struct OnEnemyKilledEvent : IGameEvent { }

public readonly struct OnSetTutorialVisibleEvent : IGameEvent
{
    public readonly bool Show;

    public OnSetTutorialVisibleEvent(bool show)
    {
        Show = show;
    }
}

public readonly struct OnRunEndedEvent : IGameEvent
{
    public readonly RunResult Result;

    public OnRunEndedEvent(RunResult result)
    {
        Debug.Log($"Run ended with result: {result}");
        Result = result;
    }
}

#region Tutorial Events

public readonly struct OnShowCoalWaypointEvent : IGameEvent { }

public readonly struct OnSetTutorialEnemyTarget : IGameEvent
{
    public readonly int index;

    public OnSetTutorialEnemyTarget(int index)
    {
        this.index = index;
    }
}

public readonly struct OnSetEnemiesFlags : IGameEvent
{
    public readonly bool canAttack;
    public readonly bool canMove;

    public OnSetEnemiesFlags(bool canAttack = false, bool canMove = false)
    {
        this.canAttack = canAttack;
        this.canMove = canMove;
    }
}

public readonly struct OnSetShieldsActiveEvent : IGameEvent
{
    public readonly bool active;

    public OnSetShieldsActiveEvent(bool active)
    {
        this.active = active;
    }
}

public readonly struct OnActivateGoldWagon : IGameEvent { }

public readonly struct OnAdvanceTutorialStepByClick : IGameEvent { }

public readonly struct OnAdvanceTutorialStep : IGameEvent { }

public readonly struct OnSkipTutorial : IGameEvent { }

public readonly struct OnSetFirstHeal : IGameEvent
{
    public readonly bool active;

    public OnSetFirstHeal(bool active)
    {
        this.active = active;
    }
}

public readonly struct OnShowArrowImage : IGameEvent
{
    public readonly bool show;

    public OnShowArrowImage(bool show)
    {
        this.show = show;
    }
}

public readonly struct OnShowClickImage : IGameEvent
{
    public readonly bool show;

    public OnShowClickImage(bool show)
    {
        this.show = show;
    }
}

public readonly struct OnTutorialTextRevealStateEvent : IGameEvent
{
    public readonly bool Revealing;

    public OnTutorialTextRevealStateEvent(bool revealing)
    {
        Revealing = revealing;
    }
}

public readonly struct OnCompleteTutorialTextEvent : IGameEvent { }

public readonly struct OnSetRepairGoldWagonWaypoint : IGameEvent
{
    public readonly bool Show;

    public OnSetRepairGoldWagonWaypoint(bool show)
    {
        Show = show;
    }
}

public readonly struct OnSetRepairIconEnabledEvent : IGameEvent
{
    public readonly bool Enabled;

    public OnSetRepairIconEnabledEvent(bool enabled)
    {
        Enabled = enabled;
    }
}

public readonly struct OnShowRepairIconEvent : IGameEvent
{
    public readonly bool Show;

    public OnShowRepairIconEvent(bool enabled)
    {
        Show = enabled;
    }
}

public readonly struct OnSetTutorialFinished : IGameEvent { }

#endregion

#region Player Events

public readonly struct OnFinishPlayerInitialize : IGameEvent { }

#endregion

#region Weapons Events

#region Wichester Events

public readonly struct OnUpdateEnemiesDamage : IGameEvent
{
    public readonly List<Enemy> Enemies;

    public OnUpdateEnemiesDamage(List<Enemy> enemies)
    {
        Enemies = enemies;
    }
}

public readonly struct OnUpdateWinchesterLegadoLeftPoint : IGameEvent
{
    public readonly int point;

    public OnUpdateWinchesterLegadoLeftPoint(int point)
    {
        this.point = point;
    }
}

public readonly struct OnUnlockWinchesterLegado : IGameEvent { }

public readonly struct OnWinchesterDetectedDeadEnemy : IGameEvent { }

#endregion

#region Spencer Events

public readonly struct OnSpencerDetectedDeadEnemy : IGameEvent { }

public readonly struct OnUpdatedSpencerLegado : IGameEvent { }

public readonly struct OnUnlockSpencerLegado : IGameEvent { }

#endregion

#region Coach Events

public readonly struct OnCoachDetectedDeadEnemy : IGameEvent
{
    public readonly int point;

    public OnCoachDetectedDeadEnemy(int point)
    {
        this.point = point;
    }
}

public readonly struct OnUpdatedCoachLegado : IGameEvent { }

public readonly struct OnUnlockCoachLegado : IGameEvent { }

#endregion

#region Colt Events

public readonly struct OnColtDetectedDeadEnemy : IGameEvent
{
    public readonly int point;

    public OnColtDetectedDeadEnemy(int point)
    {
        this.point = point;
    }
}

public readonly struct OnUpdatedColtLegado : IGameEvent { }

public readonly struct OnUnlockColtLegado : IGameEvent { }

#endregion

#endregion

#region Wagon Events

public readonly struct OnFinishBuildTrain : IGameEvent
{
    public readonly List<WagonBrain> wagonBrains;

    public OnFinishBuildTrain(List<WagonBrain> wagonBrains)
    {
        this.wagonBrains = wagonBrains;
    }
}

#endregion

public readonly struct OnWagonAddedToDisplayEvent : IGameEvent
{
    public readonly string AnchorKey;
    public OnWagonAddedToDisplayEvent(string anchorKey)
    {
        AnchorKey = anchorKey;
    }
}