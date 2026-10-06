using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

public class EnemiesController : MonoBehaviour
{
    [Header("Levels")]
    [SerializeField] private List<LevelSpawnsData> levelList = new();
    [SerializeField] private SpawnZone spawnZone;

    private readonly List<Enemy> activeEnemies = new();
    private readonly HashSet<Enemy> activeEnemySet = new();
    private ReadOnlyCollection<Enemy> readOnlyActiveEnemies;
    private readonly List<EnemyData> spawnPool = new();
    private readonly Dictionary<EnemyData, int> actualSpawnCounts = new();
    private readonly TrainRanges trainRanges = new();

    private SpawnController spawnController;
    private SessionConfig sessionConfig;
    private LevelSpawnsData currentLevelData;
    private Camera cam;
    private float spawnInterval;
    private int maxAliveEnemies;
    private float timer;
    private bool isInitialized;
    private bool isTutorialRun;
    private bool hasReportedAutomaticSpawnError;

    public IReadOnlyCollection<Enemy> ActiveEnemies => readOnlyActiveEnemies;
    public bool IsSpawningPaused { get; private set; }
    public bool AreWavesStarted { get; private set; }

    private void Awake()
    {
        readOnlyActiveEnemies = activeEnemies.AsReadOnly();
    }

    private void OnEnable()
    {
        ServiceLocator.Register(this);
        EventBus.Subscribe<OnEnemyBecameInactiveEvent>(OnEnemyBecameInactive);
        EventBus.Subscribe<OnSpawnEnemyEvent>(SpawnRequestedEnemy);
        EventBus.Subscribe<OnStartSpawningEnemiesEvent>(SetSpawningState);
        EventBus.Subscribe<OnSetTutorialFinished>(ClearTutorial);
        StartCoroutine(InitializeAfterEnable());
    }

    private void OnDisable()
    {
        StopWaves();
        DespawnAll();
        EventBus.Unsubscribe<OnEnemyBecameInactiveEvent>(OnEnemyBecameInactive);
        EventBus.Unsubscribe<OnSpawnEnemyEvent>(SpawnRequestedEnemy);
        EventBus.Unsubscribe<OnStartSpawningEnemiesEvent>(SetSpawningState);
        EventBus.Unsubscribe<OnSetTutorialFinished>(ClearTutorial);
        ServiceLocator.Unregister<EnemiesController>();
        isInitialized = false;
        timer = 0f;
        spawnPool.Clear();
        actualSpawnCounts.Clear();
        hasReportedAutomaticSpawnError = false;
    }

    private IEnumerator InitializeAfterEnable()
    {
        yield return null;

        if (!EnsureInitialized()) yield break;

        if (!isTutorialRun && !AreWavesStarted)
            StartWaves();

        if (AreWavesStarted && !IsSpawningPaused)
            TrySpawnHorde();
    }

    private void Update()
    {
        if (!isInitialized || !AreWavesStarted || IsSpawningPaused) return;

        if (spawnInterval <= 0f)
        {
            ReportAutomaticSpawnError("Enemy spawn interval must be greater than zero.");
            return;
        }

        timer += Time.deltaTime;
        if (timer < spawnInterval)
            return;

        if (spawnPool.Count > 0 && activeEnemies.Count < maxAliveEnemies)
            TrySpawnHorde();

        timer = 0f;
    }

    public void RegisterEnemy(Enemy enemy)
    {
        if (enemy == null || enemy.IsDead || !activeEnemySet.Add(enemy)) return;

        activeEnemies.Add(enemy);
    }

    public void PauseSpawning()
    {
        IsSpawningPaused = true;
    }

    public void ResumeSpawning()
    {
        IsSpawningPaused = false;
    }

    public void StartWaves()
    {
        AreWavesStarted = true;
        IsSpawningPaused = false;
    }

    public void StopWaves()
    {
        AreWavesStarted = false;
        timer = 0f;
    }

    public void DespawnAll()
    {
        Enemy[] snapshot = activeEnemies.ToArray();
        foreach (Enemy enemy in snapshot)
            DespawnSpecific(enemy);

        activeEnemies.Clear();
        activeEnemySet.Clear();
    }

    public void DespawnSpecific(Enemy enemy)
    {
        if (enemy == null) return;

        enemy.Despawn();
    }

    private void ClearTutorial(OnSetTutorialFinished ev)
    {
        DespawnAll();
    }

    private void OnEnemyBecameInactive(OnEnemyBecameInactiveEvent eventData)
    {
        Enemy enemy = eventData.Enemy;
        if (ReferenceEquals(enemy, null) || !activeEnemySet.Remove(enemy)) return;

        activeEnemies.Remove(enemy);
    }

    private bool EnsureInitialized()
    {
        if (isInitialized) return true;

        if (!ServiceLocator.TryGet(out spawnController) || spawnController == null)
        {
            Debug.LogError("EnemiesController requires an active SpawnController.", this);
            return false;
        }

        if (!ServiceLocator.TryGet(out sessionConfig) || sessionConfig == null)
        {
            Debug.LogError("EnemiesController requires a registered SessionConfig.", this);
            return false;
        }

        if (levelList == null || levelList.Count == 0)
        {
            Debug.LogError("EnemiesController has no level spawn data assigned.", this);
            return false;
        }

        int levelIndex = Mathf.Clamp(sessionConfig.CurrentLevel, 0, levelList.Count - 1);
        currentLevelData = levelList[levelIndex];
        if (currentLevelData == null || currentLevelData.prefab == null)
        {
            Debug.LogError("EnemiesController level data or enemy prefab is missing.", this);
            return false;
        }

        spawnInterval = Mathf.Max(0f, currentLevelData.spawnInterval);
        maxAliveEnemies = Mathf.Max(0, currentLevelData.maxAliveEnemies);
        BuildSpawnPool();
        cam = Camera.main;
        isTutorialRun = GameManager.Instance != null && GameManager.Instance.IsTutorial;
        isInitialized = true;
        return true;
    }

    private void BuildSpawnPool()
    {
        spawnPool.Clear();
        if (currentLevelData.spawneables == null) return;

        foreach (SpawnEntry entry in currentLevelData.spawneables)
        {
            if (entry == null || entry.enemyData == null || entry.weight <= 0) continue;

            for (int i = 0; i < entry.weight; i++)
                spawnPool.Add(entry.enemyData);
        }
    }

    private void TrySpawnHorde()
    {
        if (!EnsureInitialized() || currentLevelData.MaxHordeSpawn <= 0 || activeEnemies.Count >= maxAliveEnemies)
            return;

        if (spawnInterval <= 0f)
        {
            ReportAutomaticSpawnError("Enemy spawn interval must be greater than zero.");
            return;
        }

        if (spawnPool.Count == 0)
        {
            ReportAutomaticSpawnError("The current level has no weighted enemy spawn entries.");
            return;
        }

        if (spawnZone == null)
        {
            ReportAutomaticSpawnError("EnemiesController requires a SpawnZone for automatic waves.");
            return;
        }

        if (cam == null)
            cam = Camera.main;
        if (cam == null)
        {
            ReportAutomaticSpawnError("EnemiesController requires a MainCamera for automatic waves.");
            return;
        }

        (float positive, float negative) = trainRanges.SetRanges(50f, Vector3.zero);
        Vector3 spawnPosition = spawnZone.GetRandomPoint(positive, negative);

        for (int i = 0; i < currentLevelData.MaxHordeSpawn && activeEnemies.Count < maxAliveEnemies; i++)
        {
            if (CameraView.IsOutsideCamera(spawnPosition, cam))
                SpawnWeightedEnemy(spawnPosition);

            spawnPosition = spawnZone.GetRandomPoint(positive, negative);
        }
    }

    private void SpawnWeightedEnemy(Vector3 position)
    {
        if (spawnPool.Count == 0) return;

        EnemyData data = spawnPool[Random.Range(0, spawnPool.Count)];
        SpawnEnemy(data, position, false);
    }

    private void SpawnRequestedEnemy(OnSpawnEnemyEvent spawnEvent)
    {
        if (spawnEvent == null || !EnsureInitialized()) return;

        Enemy enemy = SpawnEnemy(spawnEvent.Enemy, spawnEvent.Position, true);
        if (enemy == null) return;

        EventBus.Publish(new OnSetTutorialEnemyTarget(1));
    }

    private Enemy SpawnEnemy(EnemyData data, Vector3 position, bool tutorialEnemy)
    {
        if (data == null || !EnsureInitialized()) return null;

        if (data.movement is EnemyMovementSlowSO)
        {
            position.x = 60f;
            position.z = position.z < 0
                ? Random.Range(-(float)RangeType.Long, -(float)RangeType.Long*1.5f)
                : Random.Range((float)RangeType.Long, (float)RangeType.Long*1.5f);
        }

        Enemy enemy = spawnController.SpawnEnemy(
            currentLevelData.prefab,
            data,
            position,
            Quaternion.identity
        );
        if (enemy == null) return null;

        if (tutorialEnemy)
            enemy.SetTutorialEnemy();

        RegisterEnemy(enemy);
        TrackSpawn(data);
        return enemy;
    }

    private void TrackSpawn(EnemyData data)
    {
        if (!actualSpawnCounts.ContainsKey(data))
            actualSpawnCounts[data] = 0;
        actualSpawnCounts[data]++;

        var counts = actualSpawnCounts.Select(kvp => $"{kvp.Key.name}: {kvp.Value}");
        Debug.Log($"Spawned {data.name}. Current spawn counts: {string.Join(", ", counts)}");
    }

    private void SetSpawningState(OnStartSpawningEnemiesEvent spawnEvent)
    {
        if (spawnEvent.Can)
            StartWaves();
        else
            PauseSpawning();
    }

    private void ReportAutomaticSpawnError(string message)
    {
        if (hasReportedAutomaticSpawnError) return;

        hasReportedAutomaticSpawnError = true;
        Debug.LogError(message, this);
    }
}