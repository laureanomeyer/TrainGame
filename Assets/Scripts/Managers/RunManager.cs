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

    private TrainData trainCopyData;
    public TrainData TrainCopyData => trainCopyData;
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
        if (trainCopyData != null) trainCopyData.ResetBuffsList();
        trainCopyData = GameManager.Instance.TrainData;

    }
    public void OnTrainReady()
    {
        GameObject obj = Instantiate(mapManagerPrefab, TrainCopyData.TailPosition.position, TrainCopyData.TailPosition.rotation);
        mapManager = obj.GetComponent<MapManager>();
        mapManager.Initialize(mapStartLocation);
    }
    public void SetLocoBrain(LocomotiveBrain locomotion)
    {
        this.locomotiveBrain = locomotion;
    }
}

