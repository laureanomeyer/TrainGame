using UnityEngine;

public class TrainManager : MonoBehaviour
{
    [SerializeField] private Transform tail;
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
    void CreateWagon()
    {
        GameObject WagonInstance = Instantiate(WagonPrefab);
        AddWagon(tail, WagonInstance.GetComponent<WagonMovement>());
    }
    void AddWagon(Transform head, WagonMovement wagon)
    {
        wagon.Initialize(head);
        tail = wagon.Tail;
    }
}
