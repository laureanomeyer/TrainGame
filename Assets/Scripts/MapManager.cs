using UnityEngine;

public class MapManager : MonoBehaviour
{
    [SerializeField] private SharedData sharedData;
    [SerializeField] private Transform endLocation;
    [SerializeField] private Transform startLocation;
    private MapTile[] tiles;

    void Start()
    {
        endLocation = sharedData.tailPosition;
    }

    void Update()
    {
        Move();
    }

    void Move()
    {

    }

    void CreateTile()
    {

    }
}
