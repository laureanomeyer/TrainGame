
using UnityEngine;

public class GameSession
{
    private readonly LocomotiveStatsSO baseStats;
    private readonly LocomotiveStatsSO baseMultStats;

    public PlayerData _PlayerData { get; private set; }
    public TrainData _TrainData { get; private set; }
    public StatSystem _StatSystem { get; private set; }
    public SessionConfig _SessionConfig { get; private set; }
    public ICinematicActorRegistry _CinematicActorRegistry { get; private set; }

    public GameSession(LocomotiveStatsSO baseStats, LocomotiveStatsSO baseMultStats)
    {
        this.baseStats = baseStats;
        this.baseMultStats = baseMultStats;

        _PlayerData = new PlayerData();
        _TrainData = new TrainData(baseStats, baseMultStats);
        _SessionConfig = new SessionConfig();
        _StatSystem = new StatSystem(baseStats, _TrainData);

        ServiceLocator.Register<PlayerData>(_PlayerData);
        ServiceLocator.Register<TrainData>(_TrainData);
        ServiceLocator.Register<StatSystem>(_StatSystem);
        ServiceLocator.Register<SessionConfig>(_SessionConfig);
        ServiceLocator.Register<ICinematicActorRegistry>(new CinematicActorRegistry());
    }

    public void RebuildStatsSystem()
    {
        _StatSystem = new StatSystem(baseStats, _TrainData);
        EventBus.Publish(new OnStatChangedEvent());
    }

    public void Reset()
    {
        ServiceLocator.Clear();

    }
}

