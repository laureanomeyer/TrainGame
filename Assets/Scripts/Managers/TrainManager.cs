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
        CreateLocomotive();
        CreateWagon();
        RunManager.Instance.trainM = this;
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
    }
    public void CreateWagon() //Instancia un vagon REEMPLAZAR POR UNA POOL
    {
        GameObject WagonInstance = Instantiate(WagonPrefab, tail.position, tail.rotation);
        WagonMovement wagon = WagonInstance.GetComponent<WagonMovement>();
        WagonBrain wagonBrain = WagonInstance.GetComponent<WagonBrain>();

        AddWagon(tail, wagon);
        wagonsList.Add(wagon);

        RunManager.Instance.TrainCopyData.SetWagonList(wagonsList);

        //Actualmente no funciona, pero, deberia sumar los TrainStats de TrainData con los TrainStats de los vagones que posea buffers
        //NO FUNCIONA - PERO PARA SUMAR LAS STATS DE UN VAGON LA IDEA ES ESTA !!!!!!
        //RunManager.Instance.trainM.trainData.stats += BufferList[0].StatsBuff;

        
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
