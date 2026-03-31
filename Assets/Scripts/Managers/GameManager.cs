using System.Collections.Generic;
using UnityEngine;
[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private float speed;
    [SerializeField] private Transform tailPosition;
    [SerializeField] private Transform initialTailPosition;
    [SerializeField] private GameObject mapManagerPrefab;
    [SerializeField] private Transform mapStartLocation;
    private List<IWagon> wagonList;
    private MapManager mapManager;

    public float Speed => speed;
    public Transform TailPosition => tailPosition;
    public List<IWagon> WagonList => wagonList;
    public Transform InitialTailPosition => initialTailPosition;


    void Awake()
    {
        if (Instance != null && Instance != this) 
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;

        }

    }
   /* private void Update()
    {
        Debug.Log(Speed);
    }*/

    public void OnTrainReady()
    {

        GameObject obj = Instantiate(mapManagerPrefab, tailPosition.position, tailPosition.rotation);
        mapManager = obj.GetComponent<MapManager>();
        mapManager.Initialize(mapStartLocation);
    }

    public void SetSpeed(float speed)
    {
        this.speed = speed;
    }
    public void SetTrainTail(Transform tail)
    {
        tailPosition = tail;

    }
    public void SetWagonList(List<IWagon> wagonList)
    {
        this.wagonList = wagonList;
    }
}
