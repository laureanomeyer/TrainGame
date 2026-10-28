using UnityEngine;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    private static readonly Quaternion TileRotation = Quaternion.Euler(0f, 90f, 0f);

    [Header("Floor tiles config")]
    [SerializeField] int maxLevelWithDesert = 4;
    [SerializeField] int maxLevelWithGrass = 8;
    [SerializeField] private GameObject desertTilePrefab;
    [SerializeField] private GameObject grassTilePrefab;
    [SerializeField] private GameObject caveTilePrefab;
    private GameObject currentTile;

    [Header("Level end tiles (montaña)")]
    [Tooltip("Prefab con MapTile (NO MapChangingTile) + LevelEndTile.")]
    [SerializeField] private LevelEndTile desertEndTilePrefab;
    [SerializeField] private LevelEndTile grassEndTilePrefab;
    [SerializeField] private LevelEndTile caveEndTilePrefab;

    [Tooltip("Segundos entre el fin del timer y la llegada de la entrada a la locomotora. " +
             "Le da tiempo a la cámara a encuadrar antes de que el tren entre.")]
    [SerializeField] private float arrivalDelayAfterRunEnd = 1.5f;

    [SerializeField] private Transform endLocation;
    [SerializeField] private Transform startLocation;
    [SerializeField] private List<MapTile> tilesMap;
    [SerializeField] private MapTile head;
    [SerializeField] private GameObject deathWall;

    [Header("Victory threshold (intervalo de madera) por bioma")]
    [SerializeField] private bool showVictoryThresholdInDesert = true;
    [SerializeField] private bool showVictoryThresholdInGrass = true;
    [SerializeField] private bool showVictoryThresholdInCave = true;

    public bool ShowVictoryThreshold { get; private set; } = true;
    public bool IsInitialized => initialized;

    private bool initialized = false;

    private LevelEndTile endTilePrefab;
    private LevelEndTile endTileInstance;
    private bool endTileSpawned;
    private SceneRunController runController;

    private float scrollMultiplier = 1f;
    public void SetScrollMultiplier(float multiplier) => scrollMultiplier = Mathf.Clamp01(multiplier);

    private void OnEnable() => EventBus.Subscribe<OnRunEndedEvent>(OnRunEnded);
    private void OnDisable() => EventBus.Unsubscribe<OnRunEndedEvent>(OnRunEnded);

    public void Initialize(Transform startLocation)
    {
        this.startLocation = startLocation;

        endLocation = RunManager.Instance.TrainTail;

        var sessionConfigRef = ServiceLocator.Get<SessionConfig>();
        int level = sessionConfigRef.CurrentLevel;

        if (level <= maxLevelWithDesert)
        {
            currentTile = desertTilePrefab;
            endTilePrefab = desertEndTilePrefab;
        }
        else if (level > maxLevelWithDesert && level < maxLevelWithGrass)
        {
            currentTile = grassTilePrefab;
            endTilePrefab = grassEndTilePrefab;
        }
        else
        {
            currentTile = caveTilePrefab;
            endTilePrefab = caveEndTilePrefab;
        }

        if (endTilePrefab == null)
            Debug.LogWarning($"[MapManager] No hay tile de fin asignado para el nivel {level}.");

        for (int i = 0; i < RunManager.Instance.ActiveWagons.Count + 3; i++)
        {
            GenerateMap();
        }

        for (int i = 1; i < tilesMap.Count; i++) 
        {
            if (!tilesMap[i].IsMapHead)
            {
                if (tilesMap[i] is TileChange changingTale)
                {
                    changingTale.SetUpWithMesh(tilesMap[i - 1].Tail);
                }
                tilesMap[i].transform.position = tilesMap[i - 1].Tail.position;
            }
        }

        if (level <= maxLevelWithDesert)
        {
            currentTile = desertTilePrefab;
            endTilePrefab = desertEndTilePrefab;
            ShowVictoryThreshold = showVictoryThresholdInDesert;
        }
        else if (level > maxLevelWithDesert && level < maxLevelWithGrass)
        {
            currentTile = grassTilePrefab;
            endTilePrefab = grassEndTilePrefab;
            ShowVictoryThreshold = showVictoryThresholdInGrass;
        }
        else
        {
            currentTile = caveTilePrefab;
            endTilePrefab = caveEndTilePrefab;
            ShowVictoryThreshold = showVictoryThresholdInCave;
        }

        GameObject.Instantiate(deathWall, new Vector3(endLocation.position.x - tilesMap[0].Offset * 3, endLocation.position.y, endLocation.position.z), Quaternion.Euler(0, 0, 0));
        initialized = true;
    }

    void Update()
    {
        if (!initialized) return;

        MoveTiles();

        if (endTileSpawned) SyncTimerToEndTile();
        else TryScheduleEndTile();

        if (scrollMultiplier > 0f)
            RecycleTile();
    }

    void MoveTiles()
    {
        float speed = RunManager.Instance.TrainSpeed * scrollMultiplier;

        foreach (MapTile tile in tilesMap)
        {
            tile.MoveHead(endLocation.position - tilesMap[0].OffsetVect * 4, speed);
            tile.Move();
        }
    }

    void RecycleTile()
    {
        if (tilesMap.Count == 0) return;
        if (head.Tail.position.x <= endLocation.position.x - tilesMap[0].Offset * 3)
        {
            MapTile recycled = head;

            tilesMap.RemoveAt(0);
            recycled.SetUp(tilesMap[tilesMap.Count - 1].Tail);
            recycled.transform.position = tilesMap[tilesMap.Count - 1].Tail.position;
            tilesMap.Add(recycled);
            head = tilesMap[0];
            tilesMap[0].SetTail();


            if (endTileSpawned)
                recycled.gameObject.SetActive(false);
        }
    }

    void GenerateMap()
    {
        Vector3 spawnPosition;
        if (tilesMap.Count == 0)
            spawnPosition = new Vector3(0, 0, 0);
        else
            spawnPosition = tilesMap[tilesMap.Count - 1].Tail.position;

        GameObject tile = Instantiate(currentTile, spawnPosition, TileRotation);
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



    private bool TryGetRunController()
    {
        if (runController == null)
            runController = ServiceLocator.Get<SceneRunController>();
        return runController != null;
    }

    private float LocomotiveX =>
        RunManager.Instance.LocomotiveBrain != null
            ? RunManager.Instance.LocomotiveBrain.transform.position.x
            : endLocation.position.x;

    private void TryScheduleEndTile()
    {
        if (endTilePrefab == null || !TryGetRunController()) return;

        float speed = RunManager.Instance.TrainSpeed;
        if (speed <= 0.01f) return;

        float timeToArrive = EstimateEntranceDistanceIfSpawnedNow() / speed;


        if (runController.RemainingTime + arrivalDelayAfterRunEnd <= timeToArrive)
            SpawnEndTile();
    }


    private float EstimateEntranceDistanceIfSpawnedNow()
    {

        MapTile prefabTile = endTilePrefab.GetComponent<MapTile>();
        float prefabOffset = prefabTile != null && prefabTile.Tail != null
            ? Vector3.Distance(prefabTile.transform.position, prefabTile.Tail.position)
            : 0f;

        Vector3 tilePos = tilesMap[tilesMap.Count - 1].Tail.position + new Vector3(prefabOffset, 0f, 0f);
        Vector3 entrancePos = tilePos + TileRotation * endTilePrefab.LocalEntranceOffset;

        return Mathf.Max(0f, entrancePos.x - LocomotiveX);
    }

    private void SpawnEndTile()
    {
        if (endTileSpawned || endTilePrefab == null || tilesMap.Count == 0) return;
        endTileSpawned = true;

        MapTile last = tilesMap[tilesMap.Count - 1];

        GameObject obj = Instantiate(endTilePrefab.gameObject, last.Tail.position, TileRotation);
        MapTile endTile = obj.GetComponent<MapTile>();

        if (endTile == null)
        {
            Debug.LogError("[MapManager] El prefab de fin de nivel no tiene MapTile.");
            Destroy(obj);
            return;
        }

        if (endTile is TileChange)
            Debug.LogWarning("[MapManager] El tile de fin usa MapChangingTile: usá MapTile, si no se pisa el mesh de la montaña.");


        endTile.SetUp(last.Tail);
        obj.transform.position = last.Tail.position + endTile.OffsetVect;

        endTileInstance = obj.GetComponent<LevelEndTile>();
        tilesMap.Add(endTile);
    }


    private void SyncTimerToEndTile()
    {
        if (endTileInstance == null || endTileInstance.Entrance == null || !TryGetRunController()) return;

        float speed = RunManager.Instance.TrainSpeed;
        if (speed <= 0.01f) return;

        float distance = endTileInstance.Entrance.position.x - LocomotiveX;
        runController.SyncTimeUntilEnd(distance / speed - arrivalDelayAfterRunEnd);
    }

    private void OnRunEnded(OnRunEndedEvent evt)
    {

        if (evt.Result == RunResult.Victory)
            SpawnEndTile();
    }
}