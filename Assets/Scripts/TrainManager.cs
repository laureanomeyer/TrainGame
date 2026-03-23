using UnityEngine;

public class TrainManager : MonoBehaviour
{
    [SerializeField] private Transform tail; //Final del tren
    [SerializeField] private GameObject WagonPrefab; 

    private void Start()
    {
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
        AddWagon(tail, WagonInstance.GetComponent<WagonMovement>());
    }
    void AddWagon(Transform head, WagonMovement wagon) //Inicializa el vagon
    {
        wagon.Initialize(head);
        tail = wagon.Tail;
    }
}
