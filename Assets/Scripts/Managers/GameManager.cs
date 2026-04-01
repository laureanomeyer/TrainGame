using System.Collections.Generic;
using UnityEngine;
[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] private GameObject mapManagerPrefab;
    [SerializeField] private Transform mapStartLocation;
    private LocomotiveBrain locomotiveBrain;
    private MapManager mapManager;
    private TrainData trainData;
    public TrainManager trainM;
    public LocomotiveBrain locoM;

    public TrainData TrainData => trainData;
    public LocomotiveBrain LocomotiveBrain => locomotiveBrain;

    void Awake()
    {
        if (Instance != null && Instance != this) 
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
            trainData = new TrainData();
        }

    }
    public void OnTrainReady()
    {
        GameObject obj = Instantiate(mapManagerPrefab, TrainData.TailPosition.position, TrainData.TailPosition.rotation);
        mapManager = obj.GetComponent<MapManager>();
        mapManager.Initialize(mapStartLocation);
    }
    public void SetLocoBrain(LocomotiveBrain locomotion)
    {
        this.locomotiveBrain = locomotion;
    }
}
