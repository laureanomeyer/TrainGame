using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[DefaultExecutionOrder(-99)]
public class RunManager : MonoBehaviour
{
    public static RunManager Instance;
    [SerializeField] private GameObject mapManagerPrefab;
    [SerializeField] private Transform mapStartLocation;
    private LocomotiveBrain locomotiveBrain;
    private MapManager mapManager;
    public TrainManager trainM;
    public LocomotiveBrain locoM;

    private TrainData trainData;
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
        }
            trainData = new TrainData(GameManager.Instance.baseStats);

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

