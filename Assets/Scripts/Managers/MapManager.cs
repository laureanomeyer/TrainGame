using UnityEngine;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private Transform endLocation;
    [SerializeField] private Transform startLocation;
    [SerializeField] private List<MapTile> tilesMap;
    [SerializeField] private MapTile head;
    [SerializeField] private GameObject deathWall;
    [SerializeField] private Mesh[] variatons;

    private bool initialized = false;
   

    public void Initialize(Transform startLocation)
    {
        this.startLocation = startLocation;

        endLocation = RunManager.Instance.TrainTail;

        for (int i = 0; i < RunManager.Instance.ActiveWagons.Count + 3; i++)
        {
            GenerateMap();
        }

        for (int i = 1; i < tilesMap.Count; i++)  // empieza en 1
        {
            if (!tilesMap[i].IsMapHead)
            {
                tilesMap[i].SetUpWithMesh(tilesMap[i - 1].Tail, variatons[Random.Range(0, variatons.Length)]);
                tilesMap[i].transform.position = tilesMap[i - 1].Tail.position;
            }
        }
        GameObject.Instantiate(deathWall, new Vector3(endLocation.position.x - tilesMap[0].Offset * 3, endLocation.position.y, endLocation.position.z), Quaternion.Euler(0, 0, 0));
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
            tile.MoveHead(endLocation.position - tilesMap[0].OffsetVect * 4, RunManager.Instance.TrainSpeed);
            tile.Move();
        }
    }

    void RecycleTile()
    {
        if (tilesMap.Count == 0) return;
        if (head.Tail.position.x <= endLocation.position.x - tilesMap[0].Offset * 3)
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
            spawnPosition = new Vector3 (0, 0, 0);
        else
        {
            MapTile last = tilesMap[tilesMap.Count - 1];
            spawnPosition = tilesMap[tilesMap.Count - 1].Tail.position;
        }

        Vector3 direction = (RunManager.Instance.TrainTail.position -
                     RunManager.Instance.TrainTail.position).normalized;
        Quaternion rotation = Quaternion.LookRotation(direction);
        rotation = new Quaternion(0, 1, 0, 1f);
        

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
