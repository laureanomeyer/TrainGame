using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TrainManager : MonoBehaviour
{
    [Header("Ending point")]
    [SerializeField] private Transform tail;

    [Header("Wagons")]
    [SerializeField] private GameObject WagonPrefab;
    [SerializeField] private GameObject GoldWagonPrefab;

    [SerializeField] private GameObject LocomotivePrefab;

    private List<GameObject> wagonsCreated = new List<GameObject>();

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

        GameEvents.OnChangeTrainData += ChangeWagonsInTrainData;
    }

    private void Start()
    {
        RunManager.Instance.OnTrainReady();

    }

    public void CreateTrain()
    {
        CreateLocomotive();
        CreateWagons();
        CreateGoldWagon();
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
            CreateWagon(wagon.Prefab);
        }
    }


    public void CreateWagon(GameObject wagonToCreate)
    {
        GameObject wagonInstance = Instantiate(wagonToCreate, tail.position, tail.rotation);
        WagonMovement wagon = wagonInstance.GetComponent<WagonMovement>();
        WagonBrain wagonBrain = wagonInstance.GetComponent<WagonBrain>();

        wagonsList.Add(wagon);

        GameManager.Instance.TrainData.AddToBufferList(wagonBrain);
        GameManager.Instance.UpdateTrainData();
        AddWagon(tail, wagon);

        wagonsCreated.Add(wagonInstance);
    }
    public void CreateGoldWagon()
    {
        GameObject WagonInstance = Instantiate(GoldWagonPrefab, tail.position, tail.rotation);
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

    private void ChangeWagonsInTrainData()
    {
        if (wagonsList.Count > 0)
        {
            foreach (var wagon in wagonsCreated)
            {
                if (wagon.GetComponent<WagonBrain>().HPController.IsBroken)
                {
                    wagonsCreated.Remove(wagon);
                    Debug.Log("Se removio un vagon");
                }
                else
                {
                    Debug.Log("No se removio ningun vagon");
                }

            }

            Debug.Log("Elementos en la lista" + wagonsCreated.Count);
            //GameManager.Instance.TrainData.ChangedWagonIDList(wagonsCreated);
        }
       
    }

    private void OnDestroy()
    {
        GameEvents.OnChangeTrainData -= ChangeWagonsInTrainData;
    }
}
