using UnityEngine;
using System.Collections.Generic;

public class TrainManager : MonoBehaviour
{
    [SerializeField] private Transform tail;
    [SerializeField] private GameObject WagonPrefab;
    [SerializeField] private GameObject LocomotivePrefab;
    private List<IWagon> wagonsList;
    private TrainData trainData;
    private WagonMovement lastWagon;

    public TrainData TrainData => trainData;

    private void Awake()
    {
        wagonsList = new List<IWagon>();

        CreateLocomotive();
        
        CreateWagon();

        RunManager.Instance.trainM = this;
        RunManager.Instance.OnTrainReady();
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

        AddWagon(tail, wagon);
        wagonsList.Add(wagon);
        RunManager.Instance.TrainData.SetWagonList(wagonsList);
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
