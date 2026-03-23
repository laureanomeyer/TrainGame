using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MapManager : MonoBehaviour
{
    [SerializeField] private SharedData sharedData;
    [SerializeField] private Transform endLocation;
    [SerializeField] private Transform startLocation;
    [SerializeField] private List<MapTile> tilesMap; 
    private MapTile[] tiles;

    void Start()
    {
        endLocation = sharedData.tailPosition;
    }

    void Update()
    {
        MoveTiles();
        RecycleTile();
        Debug.Log(sharedData.tailPosition.position.x);
    }

    void MoveTiles()
    {
        float movement = sharedData.speed * Time.deltaTime;
        foreach (MapTile tile in tilesMap) 
        {
            tile.transform.position += Vector3.left * movement;
        }
    }

    void RecycleTile()
    {
        if (tilesMap.Count == 0) return;

        MapTile firstTile = tilesMap[0];

        if (firstTile.IsPastPoint(sharedData.tailPosition.position.x))
        {
            MapTile lasTile = tilesMap[tilesMap.Count - 1];

            firstTile.PlaceAfter(lasTile);
            tilesMap.RemoveAt(0);
            tilesMap.Add(lasTile);

        }

        
    }
}
