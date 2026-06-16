
using UnityEngine;

public class GameSession
{
    private readonly LocomotiveStatsSO baseStats;
    private readonly LocomotiveStatsSO baseMultStats;

    public PlayerData PlayerData { get; private set; }
    public TrainData TrainData { get; private set; }
    public StatSystem StatSystem { get; private set; }
    public SessionConfig SessionConfig { get; private set; }

    public GameSession(LocomotiveStatsSO baseStats, LocomotiveStatsSO baseMultStats)
    {
        this.baseStats = baseStats;
        this.baseMultStats = baseMultStats;

        PlayerData = new PlayerData();
        TrainData = new TrainData(baseStats, baseMultStats);
        SessionConfig = new SessionConfig();
        StatSystem = new StatSystem(baseStats, TrainData);
    }

    public void RebuildStatsSystem()
    {
        StatSystem = new StatSystem(baseStats, TrainData);
        GameEvents.StatChanged();
    }

    public void Reset()
    {
        this.PlayerData = new PlayerData();
        TrainData = new TrainData(baseStats, baseMultStats);
        SessionConfig = new SessionConfig();
        StatSystem = new StatSystem(baseStats, TrainData);
    }
}

