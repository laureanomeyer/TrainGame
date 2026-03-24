using UnityEngine;
using System.Collections.Generic;

public class TrainManager : MonoBehaviour
{
    [SerializeField] private Transform tail; //Final del tren
    [SerializeField] private GameObject WagonPrefab;
    [SerializeField] private SharedData sharedData;

    private List<IWagon> wagonsList;
    public List<IWagon> WagonList => wagonsList;

    private void Start()
    {
        wagonsList = new List<IWagon>();

        CreateWagon();
        CreateWagon();
        CreateWagon();
        CreateWagon();
        CreateWagon();
        CreateWagon();
        CreateWagon();
        CreateWagon();
        CreateWagon();
        CreateWagon();
    }
    void Update()
    {
       
    }
    void CreateWagon() //Instancia un vagon REEMPLAZAR POR UNA POOL
    {
        GameObject WagonInstance = Instantiate(WagonPrefab);
        WagonMovement wagon = WagonInstance.GetComponent<WagonMovement>();
        AddWagon(tail, wagon);
        wagonsList.Add(wagon);
        GameManager.Instance.SetWagonList(wagonsList);
    }
    void AddWagon(Transform head, WagonMovement wagon) //Inicializa el vagon
    {
        wagon.Initialize(head);
        tail = wagon.Tail;
        GameManager.Instance.SetTrainTail(tail);
    }
}
