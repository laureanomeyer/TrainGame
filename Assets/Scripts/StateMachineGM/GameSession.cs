
public class GameSession
{
    private readonly LocomotiveStatsSO baseStats;

    public PlayerData PlayerData { get; private set; }
    public TrainData TrainData { get; private set; }
    public StatSystem StatSystem { get; private set; }
    public SessionConfig SessionConfig { get; private set; }

    public GameSession(LocomotiveStatsSO baseStats)
    {
        this.baseStats = baseStats;
        this.PlayerData = new PlayerData();
        TrainData = new TrainData(baseStats);
        SessionConfig = new SessionConfig();
        StatSystem = new StatSystem(baseStats, TrainData);
    }

    public void RebuildStatsSystem()
    {
        StatSystem = new StatSystem(baseStats, TrainData);
    }
    public void Reset()
    {
        this.PlayerData = new PlayerData();
        TrainData = new TrainData(baseStats);
        SessionConfig = new SessionConfig();
        StatSystem = new StatSystem(baseStats, TrainData);
    }
}

