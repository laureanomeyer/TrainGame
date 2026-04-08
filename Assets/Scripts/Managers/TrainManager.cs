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
        RunManager.Instance.trainM = this;
        CreateTrain();
    }

    private void Start()
    {
        RunManager.Instance.OnTrainReady();

    }

    public void CreateTrain()
    {
        CreateLocomotive();
        CreateWagon();
        CreateWagons();
        RunManager.Instance.TrainCopyData.SetWagonList(wagonsList);

    }

    void CreateLocomotive()
    {
        GameObject LocomotiveInstance = Instantiate(LocomotivePrefab);
        var foo = LocomotiveInstance.GetComponent<LocomotiveBrain>();
        RunManager.Instance.SetLocoBrain(foo);
        wagonsList.Add(foo);
        tail = foo.TailRef;
        RunManager.Instance.TrainCopyData.SetTrainTail(tail);

    }
    public void CreateWagons()
    {
        foreach (var wagon in GameManager.Instance.TrainData.WagonsIDList)
        {
            CreateWagon();
        }
    }


    public void CreateWagon()
    {
        GameObject WagonInstance = Instantiate(WagonPrefab, tail.position, tail.rotation);
        WagonMovement wagon = WagonInstance.GetComponent<WagonMovement>();
        WagonBrain wagonBrain = WagonInstance.GetComponent<WagonBrain>();

        wagonsList.Add(wagon);

        GameManager.Instance.TrainData.AddToBufferList(wagonBrain);
        GameManager.Instance.UpdateTrainData();
        AddWagon(tail, wagon);

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
