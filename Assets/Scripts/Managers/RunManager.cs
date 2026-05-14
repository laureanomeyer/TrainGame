using System.Collections.Generic;
using UnityEngine;


[DefaultExecutionOrder(-99)]
public class RunManager : MonoBehaviour
{
    public static RunManager Instance;

    [SerializeField] private GameObject mapManagerPrefab;
    [SerializeField] private Transform mapStartLocation;
    [SerializeField] private SpawnManager spawnManager;

    private MapManager mapManager;
    private LocomotiveBrain locomotiveBrain;
    private StatSystem statSystem;

    private List<IWagon> activeWagons = new();
    private Transform trainTail;
    private float speed;

    public Transform TrainTail => trainTail;
    public float TrainSpeed => speed;
    public List<IWagon> ActiveWagons => activeWagons;
    public LocomotiveBrain LocomotiveBrain => locomotiveBrain;
    public StatSystem StatSystem => statSystem;
    public SpawnManager SpawnManager => spawnManager;

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

        statSystem = GameManager.Instance.Session.StatSystem;
        speed = statSystem.GetStat(StatType.Speed);
    }

    public void OnTrainReady(Transform tail, List<IWagon> wagons)
    {
        trainTail = tail;
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
    }

    public void SetTrainSpeed(float speed)
    {
        this.speed = speed;
    }

    public void OnWagonDestroyed(IWagonID wagon)
    {
        GameManager.Instance.Session.TrainData.RemoveWagonID(wagon);
        GameManager.Instance.Session.RebuildStatsSystem();
        statSystem = GameManager.Instance.Session.StatSystem;
    }
    public void OnRunFinished()
    {
        if (GameManager.Instance.IsFinalStation())
        {
            GameManager.Instance.Victory();
        }
        else
        {
            GameManager.Instance.GoToStore();
        }
    }
}

