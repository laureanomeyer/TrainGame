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
    private readonly Dictionary<EnemyData, (int quantActive, int quantSpawned)> enemyCounts = new();
    private readonly HashSet<Enemy> activeEnemySet = new();
    private ReadOnlyCollection<Enemy> readOnlyActiveEnemies;
    private readonly List<EnemyData> spawnPool = new();
    private readonly List<(SpawnEntryByTime entry, TimedSpawnSchedule schedule)> timedSpawnSchedules = new();
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
        EventBus.Subscribe<OnRunEndedEvent>(ChangeMovement);
        StartCoroutine(InitializeAfterEnable());
    }

    private void OnDisable()
    {
        DespawnAll();
        EventBus.Unsubscribe<OnEnemyBecameInactiveEvent>(OnEnemyBecameInactive);
        EventBus.Unsubscribe<OnSpawnEnemyEvent>(SpawnRequestedEnemy);
        EventBus.Unsubscribe<OnStartSpawningEnemiesEvent>(SetSpawningState);
        EventBus.Unsubscribe<OnSetTutorialFinished>(ClearTutorial);
        EventBus.Unsubscribe<OnRunEndedEvent>(ChangeMovement);
        ServiceLocator.Unregister<EnemiesController>();
        isInitialized = false;
        timer = 0f;
        spawnPool.Clear();
        timedSpawnSchedules.Clear();
        enemyCounts.Clear();
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
        }
        else
        {
            timer += Time.deltaTime;
            if (timer >= spawnInterval)
            {
                if (spawnPool.Count > 0 && activeEnemies.Count < maxAliveEnemies)
                    TrySpawnHorde();

                timer = 0f;
            }
        }

        UpdateTimedSpawns(Time.deltaTime);
    }

    public void RegisterEnemy(Enemy enemy)
    {
        if (enemy == null || enemy.HealthState == EnemyHealthState.Dead || !activeEnemySet.Add(enemy)) return;

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

    public bool TryGetRandomActiveEnemy(out Enemy enemy)
    {
        if (activeEnemies.Count == 0)
        {
            enemy = null;
            return false;
        }

        enemy = activeEnemies[UnityEngine.Random.Range(0, activeEnemies.Count)];
        return enemy != null;
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

        EnemyData data = enemy.Data;
        if (data != null && enemyCounts.TryGetValue(data, out var counts))
        {
            enemyCounts[data] = (Mathf.Max(0, counts.quantActive - 1), counts.quantSpawned);
        }
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
        BuildTimedSpawnSchedules();
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

    private void BuildTimedSpawnSchedules()
    {
        timedSpawnSchedules.Clear();
        if (currentLevelData.spawneablesByTime == null) return;

        foreach (SpawnEntryByTime entry in currentLevelData.spawneablesByTime)
        {
            if (entry == null || entry.enemyData == null) continue;
            if (entry.spawnInterval <= 0f || float.IsNaN(entry.spawnInterval) || float.IsInfinity(entry.spawnInterval))
            {
                ReportAutomaticSpawnError("Timed enemy spawn intervals must be greater than zero.");
                continue;
            }

            timedSpawnSchedules.Add((entry, new TimedSpawnSchedule(entry.spawnInterval, entry.onlyOnce)));
        }
    }

    private void UpdateTimedSpawns(float deltaTime)
    {
        foreach ((SpawnEntryByTime entry, TimedSpawnSchedule schedule) in timedSpawnSchedules)
        {
            if (!schedule.Advance(deltaTime) || activeEnemies.Count >= maxAliveEnemies)
                continue;

            if (!TryGetAutomaticSpawnRanges(out float positive, out float negative))
                continue;

            if (!TryGetAutomaticSpawnPosition(positive, negative, out Vector3 spawnPosition))
                continue;

            if (SpawnEnemy(entry.enemyData, spawnPosition, false) != null)
                schedule.MarkSpawned();
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

        if (!TryGetAutomaticSpawnRanges(out float positive, out float negative))
            return;

        Vector3 spawnPosition = spawnZone.GetRandomPoint(positive, negative);
        for (int i = 0; i < currentLevelData.MaxHordeSpawn && activeEnemies.Count < maxAliveEnemies; i++)
        {
            if (CameraView.IsOutsideCamera(spawnPosition, cam))
                SpawnWeightedEnemy(spawnPosition);

            spawnPosition = spawnZone.GetRandomPoint(positive, negative);
        }
    }

    private bool TryGetAutomaticSpawnRanges(out float positive, out float negative)
    {
        if (spawnZone == null)
        {
            ReportAutomaticSpawnError("EnemiesController requires a SpawnZone for automatic waves.");
            positive = 0f;
            negative = 0f;
            return false;
        }

        if (cam == null)
            cam = Camera.main;
        if (cam == null)
        {
            ReportAutomaticSpawnError("EnemiesController requires a MainCamera for automatic waves.");
            positive = 0f;
            negative = 0f;
            return false;
        }

        (positive, negative) = trainRanges.SetRanges(50f, Vector3.zero);
        return true;
    }

    private bool TryGetAutomaticSpawnPosition(float positive, float negative, out Vector3 spawnPosition)
    {
        spawnPosition = spawnZone.GetRandomPoint(positive, negative);
        return CameraView.IsOutsideCamera(spawnPosition, cam);
    }

    private void SpawnWeightedEnemy(Vector3 position)
    {
        if (spawnPool.Count == 0) return;

        EnemyData data = spawnPool[Random.Range(0, spawnPool.Count)];
        SpawnEnemy(data, position, false);
    }

    private void SpawnRequestedEnemy(OnSpawnEnemyEvent spawnEvent)
    {
        //if (spawnEvent == null || !EnsureInitialized()) return;
        if (!EnsureInitialized()) return;

        Enemy enemy = SpawnEnemy(spawnEvent.Enemy, spawnEvent.Position, true);
        if (enemy == null) return;

        EventBus.Publish(new OnSetTutorialEnemyTarget(1));
    }

    private Enemy SpawnEnemy(EnemyData data, Vector3 position, bool tutorialEnemy)
    {
        if (data == null || !EnsureInitialized()) return null;

        if (data.movement is EnemyMovementSlow)
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
        enemyCounts.TryGetValue(data, out var counts);
        enemyCounts[data] = (counts.quantActive + 1, counts.quantSpawned + 1);

        var summary = enemyCounts.Select(kvp =>
            $"{kvp.Key.name}: active={kvp.Value.quantActive}, spawned={kvp.Value.quantSpawned}");

        //Debug.Log($"Spawned {data.name}. Enemy counts: {string.Join(", ", summary)}");
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
        public void ChangeMovement(OnRunEndedEvent ev)
    {
        if (ev.Result != RunResult.None)
        {
            StopWaves();
            foreach(Enemy e in activeEnemies)
            {
                e.ChangeState(EnemyMovementState.RunEnd);
            }
            
        }
    }
}

internal sealed class TimedSpawnSchedule
{
    private readonly float interval;
    private readonly bool onlyOnce;
    private float elapsed;
    private bool completed;

    internal TimedSpawnSchedule(float interval, bool onlyOnce)
    {
        this.interval = interval;
        this.onlyOnce = onlyOnce;
    }

    internal bool Advance(float deltaTime)
    {
        if (completed) return false;

        elapsed += deltaTime;
        return elapsed >= interval;
    }

    internal void MarkSpawned()
    {
        if (onlyOnce)
            completed = true;
        else
            elapsed = 0f;
    }
}