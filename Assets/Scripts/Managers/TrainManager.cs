using UnityEngine;
using System.Collections.Generic;

public class TrainManager : MonoBehaviour
{
    [SerializeField] private Transform tail;
    [SerializeField] private GameObject WagonPrefab;
    [SerializeField] private GameObject LocomotivePrefab;
    private List<IWagon> wagonsList;
    private List<ITrainBrain> trainBrainList;
    private List<IBuffer> BufferList;

    private TrainData trainData;
    private WagonMovement lastWagon;

    public TrainData TrainData => trainData;

    private void Awake()
    {
        wagonsList = new List<IWagon>();
        trainBrainList = new List<ITrainBrain>();
        BufferList = new List<IBuffer>();

        CreateLocomotive();
        
        CreateWagon();

        RunManager.Instance.trainM = this;
        RunManager.Instance.OnTrainReady();

        //Ejemplo de suma de structs

    }

    void CreateLocomotive()
    {
        GameObject LocomotiveInstance = Instantiate(LocomotivePrefab);
        var foo = LocomotiveInstance.GetComponent<LocomotiveBrain>();
        RunManager.Instance.SetLocoBrain(foo);
        wagonsList.Add(foo);
        tail = foo.TailRef;
    }
    public void CreateWagon() //Instancia un vagon REEMPLAZAR POR UNA POOL
    {
        GameObject WagonInstance = Instantiate(WagonPrefab, tail.position, tail.rotation);
        WagonMovement wagon = WagonInstance.GetComponent<WagonMovement>();
        WagonBrain wagonBrain = WagonInstance.GetComponent<WagonBrain>();

        AddWagon(tail, wagon);
        wagonsList.Add(wagon);
        trainBrainList.Add(wagonBrain);
        BufferList.Add(wagonBrain);

        RunManager.Instance.TrainData.SetWagonList(wagonsList, trainBrainList);

        //NO FUNCIONA - PERO PARA SUMAR LAS STATS DE UN VAGON LA IDEA ES ESTA !!!!!!
        RunManager.Instance.trainM.trainData.stats += BufferList[0].StatsBuff;

        Debug.Log(trainData.stats.maxFuel);
    }
    void AddWagon(Transform head, WagonMovement wagon) //Inicializa el vagon
    {
        wagon.Initialize(head);
        tail = wagon.Tail;
        RunManager.Instance.TrainData.SetTrainTail(tail);

        if (lastWagon)
        {
            lastWagon.wagonBack.SetActive(false);
        }

        wagon.wagonBack.SetActive(true);
        lastWagon = wagon;
    }
}
