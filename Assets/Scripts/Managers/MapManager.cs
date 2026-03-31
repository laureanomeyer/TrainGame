using UnityEngine;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private Transform endLocation;
    [SerializeField] private Transform startLocation;
    [SerializeField] private List<MapTile> tilesMap;
    [SerializeField] private MapTile head;

    private bool initialized = false;
   

    public void Initialize(Transform startLocation)
    {
        this.startLocation = startLocation;

        endLocation = GameManager.Instance.TailPosition;

        for (int i = 0; i < GameManager.Instance.WagonList.Count + 5; i++)
        {
            GenerateMap();
        }

        for (int i = 1; i < tilesMap.Count; i++)  // empieza en 1
        {
            if (!tilesMap[i].IsMapHead)
            {
                tilesMap[i].SetUp(tilesMap[i - 1].Tail);
                tilesMap[i].transform.position = tilesMap[i - 1].Tail.position;
            }
        }
        initialized = true;
    }
    void Update()
    {
        if (!initialized) return;

        MoveTiles();
        RecycleTile();
    }

    void MoveTiles()
    {
        foreach (MapTile tile in tilesMap) 
        {
            tile.MoveHead(endLocation, GameManager.Instance.Speed);
            tile.Move();
        }
    }

    void RecycleTile()
    {
        if (tilesMap.Count == 0) return;
        if (head.Tail.position.x <= endLocation.position.x - tilesMap[0].Offset / 2)
        {
            tilesMap.RemoveAt(0); 
            head.SetUp(tilesMap[tilesMap.Count - 1].Tail);
            head.transform.position = tilesMap[tilesMap.Count - 1].Tail.position;
            tilesMap.Add(head);
            head = tilesMap[0];
            tilesMap[0].SetTail();
        }
    }

    void GenerateMap()
    {
        Vector3 spawnPosition;
        if (tilesMap.Count == 0)
            spawnPosition = new Vector3 (GameManager.Instance.TailPosition.position.x - 250, 0, 0);
        else
        {
            MapTile last = tilesMap[tilesMap.Count - 1];
            spawnPosition = tilesMap[tilesMap.Count - 1].Tail.position;
        }

        Vector3 direction = (GameManager.Instance.InitialTailPosition.position -
                     GameManager.Instance.TailPosition.position).normalized;
        Quaternion rotation = Quaternion.LookRotation(direction);
        rotation = new Quaternion(0, 1, 0, 1f);
        //Debug.Log(rotation);

        GameObject tile = Instantiate(tilePrefab, spawnPosition, rotation);
        MapTile mapTile = tile.GetComponent<MapTile>();

        if (tilesMap.Count == 0)
        {
            mapTile.SetTail();
            head = mapTile;
        }
        else
        {
            mapTile.SetUp(tilesMap[tilesMap.Count - 1].Tail);
        }

        tilesMap.Add(mapTile);
    }
}
