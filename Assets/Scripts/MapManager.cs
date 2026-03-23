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

    [SerializeField] private Vector3 moveDirection = Vector3.left;


    private MapTile[] tiles;

    void Start()
    {
        endLocation = sharedData.tailPosition;

        tilesMap[0].PlaceHeadAt(startLocation.position);

        for (int i = 0; i < tilesMap.Count; i++) 
        {
            tilesMap[i].PlaceAfter(tilesMap[i - 1]);
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
        float movement = sharedData.speed * Time.deltaTime;
        Vector3 delta = moveDirection.normalized * sharedData.speed * Time.deltaTime;
        foreach (MapTile tile in tilesMap) 
        {
            tile.transform.position += delta;
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
