using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-99)]
public class RunManager : MonoBehaviour
{
    public static RunManager Instance;

    public const string TailAnchorKey = "TrainTail";

    [SerializeField] private GameObject mapManagerPrefab;
    [SerializeField] private Transform mapStartLocation;

    private MapManager mapManager;
    private LocomotiveBrain locomotiveBrain;
    private StatSystem statSystem;
    private TrainData trainData;
    private ICinematicActorRegistry cinematicRegistry;

    private List<IWagon> activeWagons = new();
    private Transform trainTail;
    private float speed;

    public Transform TrainTail => trainTail;
    public float TrainSpeed => speed;
    public List<IWagon> ActiveWagons => activeWagons;
    public LocomotiveBrain LocomotiveBrain => locomotiveBrain;
    public StatSystem StatSystem => statSystem;

    private void Awake()
    {
        #region Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        #endregion

        statSystem = ServiceLocator.Get<StatSystem>();
        trainData = ServiceLocator.Get<TrainData>();
        cinematicRegistry = ServiceLocator.Get<ICinematicActorRegistry>();

        speed = statSystem.GetStat(StatType.Speed);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        cinematicRegistry?.UnregisterDynamic(TailAnchorKey);
    }

    public void OnTrainReady(Transform tail, List<IWagon> wagons)
    {
        SetTrainTail(tail);
        activeWagons = wagons;

        GameObject obj = Instantiate(mapManagerPrefab, tail.position, tail.rotation);
        mapManager = obj.GetComponent<MapManager>();
        mapManager.Initialize(mapStartLocation);
    }

    public void SetLocoBrain(LocomotiveBrain brain)
    {
        locomotiveBrain = brain;
    }

    public void SetTrainTail(Transform tail)
    {
        trainTail = tail;

        if (tail != null)
            cinematicRegistry?.RegisterDynamic(TailAnchorKey, tail);
        else
            cinematicRegistry?.UnregisterDynamic(TailAnchorKey);
    }

    public void SetTrainSpeed(float speed)
    {
        this.speed = speed;
    }

    public void OnWagonDestroyed(IWagonID wagon)
    {
        trainData.RemoveWagonID(wagon);
        GameManager.Instance.Session.RebuildStatsSystem();
    }

    public void OnRunFinished()
    {
        EventBus.Publish(new OnRunEndedEvent(RunResult.Victory));
    }
}