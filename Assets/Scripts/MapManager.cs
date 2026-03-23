using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

public class MapManager : MonoBehaviour
{
    [SerializeField] private SharedData sharedData;
    [SerializeField] private Transform endLocation;
    [SerializeField] private Transform startLocation;
    [SerializeField] private List<MapTile> tilesMap;

    void Start()
    {
        endLocation = sharedData.tailPosition;

        tilesMap[0].PlaceHeadAt(startLocation.position);

        for (int i = 0; i < tilesMap.Count; i++) 
        {
            if (!tilesMap[i].IsTail)
            {
                tilesMap[i].SetHead(tilesMap[i - 1].Tail);
                tilesMap[i].transform.position = tilesMap[i - 1].Tail.position;
            }
        }
    }

    void Update()
    {
        MoveTiles();
        RecycleTile();
        Debug.Log(sharedData.tailPosition.position.x);

    }

    void MoveTiles()
    {
        foreach (MapTile tile in tilesMap) 
        {
            tile.MoveHead(endLocation, sharedData.speed);
            tile.Move();
        }
    }

    void RecycleTile()
    {
        if (tilesMap.Count == 0) return;

        MapTile firstTile = tilesMap[0];

        if (firstTile.IsPastPoint(sharedData.tailPosition.position.x))
        {
            MapTile lastTile = tilesMap[tilesMap.Count - 1];

            firstTile.PlaceAfter(lastTile);
            tilesMap.RemoveAt(0);
            tilesMap.Add(firstTile);

        }  
    }
}
