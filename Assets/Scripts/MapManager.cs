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

    private float timer = 0;


    void Start()
    {
        endLocation = sharedData.tailPosition;
        tilesMap[0].PlaceHeadAt(startLocation.position);

        for (int i = 1; i < tilesMap.Count; i++)  // empieza en 1
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

        timer += Time.deltaTime;
        MoveTiles();
        if (timer > 3) 
        {
            RecycleTile();
        }
        

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

        for (int i = 0; i < tilesMap.Count; i++)
        {
            if (tilesMap[i].IsPastPoint(sharedData.tailPosition.position.x)) 
            {
                MapTile recycledTile = tilesMap[i];

                tilesMap.RemoveAt(i);
                tilesMap.Insert(0, recycledTile);

                recycledTile.PlaceHeadAt(startLocation.position);

                tilesMap[0].SetTail();

                for (int j = 1; j < tilesMap.Count; j++)
                {
                    tilesMap[j].SetUp(tilesMap[j - 1].Tail);
                }

                break;
            }
        }
    }
}
