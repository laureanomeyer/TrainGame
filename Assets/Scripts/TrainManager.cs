using UnityEngine;

public class TrainManager : MonoBehaviour
{
    [SerializeField] private Transform tail;
    [SerializeField] private GameObject WagonPrefab;
    float timer = 0;
    
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > 2) 
        {
            CreateWagon();
            timer = 0;
        }
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
