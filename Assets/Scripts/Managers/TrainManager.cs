using UnityEngine;
using System.Collections.Generic;

public class TrainManager : MonoBehaviour
{
    [SerializeField] private Transform tail;
    [SerializeField] private GameObject WagonPrefab;
    [SerializeField] private GameObject LocomotivePrefab;
    private List<IWagon> wagonsList;
    private List<IBuffer> BufferList; 

    private TrainData trainData;
    private WagonMovement lastWagon;

    public TrainData TrainData => trainData;

    private void Awake()
    {
        wagonsList = new List<IWagon>();
        BufferList = new List<IBuffer>();
    }

    private void Start()
    {
        RunManager.Instance.trainM = this;
        CreateTrain();
        RunManager.Instance.OnTrainReady();
    }

    public void CreateTrain()
    {
        CreateLocomotive();
        CreateWagons();
    }

    void CreateLocomotive()
    {
        GameObject LocomotiveInstance = Instantiate(LocomotivePrefab);
        var foo = LocomotiveInstance.GetComponent<LocomotiveBrain>();
        RunManager.Instance.SetLocoBrain(foo);
        wagonsList.Add(foo);
        tail = foo.TailRef;
    }
    public void CreateWagons()
    {
        CreateWagon();
        CreateWagon();
    }
    public void CreateWagon() //Instancia un vagon REEMPLAZAR POR UNA POOL
    {
        GameObject WagonInstance = Instantiate(WagonPrefab, tail.position, tail.rotation);
        WagonMovement wagon = WagonInstance.GetComponent<WagonMovement>();
        WagonBrain wagonBrain = WagonInstance.GetComponent<WagonBrain>();

        AddWagon(tail, wagon);
        wagonsList.Add(wagon);

        GameManager.Instance.AddBufferToList(wagonBrain);
        GameManager.Instance.UpdateTrainData();
        //Debug.Log(RunManager.Instance.TrainCopyData.stats.fuelOptimizer + "Run manager");
        Debug.Log(RunManager.Instance.TrainCopyData.stats.maxFuel + "Run manager");
        //Debug.Log(GameManager.Instance.TrainData.stats.fuelOptimizer + "Game manager");
        Debug.Log(GameManager.Instance.TrainData.stats.maxFuel + "Game manager");
        RunManager.Instance.TrainCopyData.SetWagonList(wagonsList);

        //RunManager.Instance.trainM.trainData.UpdateStats();
        //RunManager.Instance.LocomotiveBrain.AddTrainStats(RunManager.Instance.trainM.TrainData);
        //RunManager.Instance.TrainCopyData.AddToBufferList(wagonBrain);

    }

    void AddWagon(Transform head, WagonMovement wagon) //Inicializa el vagon
    {
        wagon.Initialize(head);
        tail = wagon.Tail;
        RunManager.Instance.TrainCopyData.SetTrainTail(tail);

        if (lastWagon)
        {
            lastWagon.wagonBack.SetActive(false);
        }

        wagon.wagonBack.SetActive(true);
        lastWagon = wagon;
    }
}
