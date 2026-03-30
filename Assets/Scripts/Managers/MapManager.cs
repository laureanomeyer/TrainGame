using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public class MapManager : MonoBehaviour
{
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private Transform endLocation;
    [SerializeField] private float speed;
    [SerializeField] private Transform startLocation;
    [SerializeField] private List<MapTile> tilesMap;
    [SerializeField] private MapTile head;

    private bool initialized = false;
   

    public void Initialize(Transform startLocation)
    {
        this.startLocation = startLocation;

        endLocation = GameManager.Instance.TailPosition;

        for (int i = 0; i < GameManager.Instance.WagonList.Count; i++)
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

        if (head.Tail.position.x <= endLocation.position.x )
        {
            tilesMap.RemoveAt(0); 
            head.SetUp(tilesMap[tilesMap.Count - 1].Tail);
            tilesMap.Add(head);
            head = tilesMap[0];
            tilesMap[0].SetTail();
        }
    }

    void GenerateMap()
    {
        GameObject tile = Instantiate(tilePrefab);
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
