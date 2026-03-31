using UnityEngine;
using System.Collections.Generic;

public class TrainManager : MonoBehaviour
{
    [SerializeField] private Transform tail; //Final del tren
    [SerializeField] private GameObject WagonPrefab;
    private WagonMovement lastWagon;

    private List<IWagon> wagonsList;
    public List<IWagon> WagonList => wagonsList;

    private void Awake()
    {
        wagonsList = new List<IWagon>();
        tail = GameManager.Instance.InitialTailPosition;

        CreateWagon();


        GameManager.Instance.OnTrainReady();
    }

    void CreateWagon() //Instancia un vagon REEMPLAZAR POR UNA POOL
    {
        GameObject WagonInstance = Instantiate(WagonPrefab, tail.position, tail.rotation);
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

        if (lastWagon)
        {
            lastWagon.wagonBack.SetActive(false);
        }

        wagon.wagonBack.SetActive(true);
        lastWagon = wagon;
    }
}
