using UnityEngine;
using System.Collections.Generic;

public class TrainManager : MonoBehaviour
{
    [SerializeField] private Transform tail; //Final del tren
    private WagonMovement lastWagon;
    [SerializeField] private GameObject WagonPrefab;

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

        GameManager.Instance.OnTrainReady();
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

        if (lastWagon)
        {
            lastWagon.wagonBack.SetActive(false);
        }
        
        tail = wagon.Tail;
        wagon.wagonBack.SetActive(true);
        GameManager.Instance.SetTrainTail(tail);

        lastWagon = wagon;
    }
}
