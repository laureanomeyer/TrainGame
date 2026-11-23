using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

// =========================================================
//  MANAGER: gestiona la conexión con Firebase y el envío
//  del JSON de cada usuario.
//
//  Estructura en Firebase:
//    analytics/meta/nextUserId   -> contador que garantiza IDs únicos
//    users/{userId}              -> JSON de UN usuario (userId = 1, 2, 3...)
// =========================================================

public class AnalyticsManager : MonoBehaviour
{
    // ----------------- Singleton -----------------
    public static AnalyticsManager Instance { get; private set; }

    // ----------------- Configuración -----------------
    [Header("Firebase")]
    [Tooltip("Dejar vacío si tu base está en us-central1. Si no, pegá la URL completa de Realtime Database.")]
    [SerializeField] private string databaseUrl = "";

    [Tooltip("SOLO WebGL: Web API Key (Configuración del proyecto > General). Se usa con la API REST.")]
    [SerializeField] private string webApiKey = "";

    [Tooltip("Cada cuántos segundos se suben los cambios pendientes.")]
    [SerializeField] private float autoFlushSeconds = 10f;

    // ----------------- Usuario actual -----------------
    public UserData CurrentUser { get; private set; }

    // ----------------- Firebase -----------------
    private IAnalyticsBackend backend;
    private string authUid => backend?.Uid;
    private bool firebaseReady => backend != null && backend.IsReady;

    // ----------------- Sincronización -----------------
    // Usuarios con cambios que todavía no se subieron.
    private readonly HashSet<UserData> dirtyUsers = new HashSet<UserData>();
    private bool flushing;
    private float timer;

    public bool IsFirebaseReady => firebaseReady;

    // =====================================================
    //  CICLO DE VIDA
    // =====================================================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // El usuario se crea en memoria ya mismo (userId = 0 = "pendiente").
        // Cuando Firebase esté listo se le asigna su ID único y se sube.
        CreateNewUser();
        _ = InitFirebaseAsync();
    }

    private void Update()
    {
        timer += Time.unscaledDeltaTime;

        if (timer >= autoFlushSeconds)
        {
            timer = 0f;
            _ = FlushAllAsync();
        }
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused) _ = FlushAllAsync();
    }

    // Mejor esfuerzo: puede que no alcance a terminar al cerrar.
    private void OnApplicationQuit()
    {
        _ = FlushAllAsync();
    }

    // =====================================================
    //  CONEXIÓN CON FIREBASE
    // =====================================================

    private async Task InitFirebaseAsync()
    {
        try
        {
#if UNITY_WEBGL
            backend = new FirebaseRestBackend(webApiKey, databaseUrl);
#else
            backend = new FirebaseSdkBackend(databaseUrl);
#endif

            bool ok = await backend.InitAsync();
            if (!ok) return;

            Debug.Log($"[Analytics] Firebase listo. Uid: {authUid}");

            await FlushAllAsync();
        }
        catch (Exception e)
        {
            Debug.LogError($"[Analytics] Error iniciando Firebase: {e}");
        }
    }

    // =====================================================
    //  USUARIOS: CREAR
    // =====================================================

    /// <summary>
    /// Crea un usuario nuevo y lo deja como CurrentUser.
    /// Su userId definitivo (único) se pide a Firebase y se asigna al subirlo.
    /// El usuario anterior se sube con sus datos finales.
    /// </summary>
    public UserData CreateNewUser()
    {
        CurrentUser = new UserData
        {
            userId = 0,                                // 0 = pendiente de asignar
            ownerUid = authUid ?? "",                  // se completa al subir si todavía no hay login
            createdAt = DateTime.UtcNow.ToString("o")
        };

        SaveUserData();
        _ = FlushAllAsync();

        Debug.Log("[Analytics] Usuario nuevo creado (esperando ID de Firebase).");
        return CurrentUser;
    }

    private bool HasCurrentUser()
    {
        if (CurrentUser == null)
        {
            Debug.LogWarning("[Analytics] No hay usuario actual. Llamá a CreateNewUser() primero.");
            return false;
        }

        return true;
    }

    // =====================================================
    //  SINCRONIZACIÓN: ENVÍO DE JSON A FIREBASE
    // =====================================================

    /// <summary>
    /// Marca al usuario actual como "con cambios".
    /// El envío real lo hace FlushAllAsync (cada pocos segundos o en momentos clave).
    /// </summary>
    public void SaveUserData()
    {
        if (CurrentUser != null)
            dirtyUsers.Add(CurrentUser);
    }

    /// <summary>
    /// Sube todos los usuarios con cambios pendientes.
    /// </summary>
    public async Task FlushAllAsync()
    {
        if (!firebaseReady || flushing) return;

        flushing = true;

        try
        {
            // Copia, porque dirtyUsers puede cambiar mientras se espera a la red.
            List<UserData> pending = new List<UserData>(dirtyUsers);

            foreach (UserData user in pending)
            {
                await UploadUserAsync(user);
            }
        }
        finally
        {
            flushing = false;
        }
    }

    private async Task UploadUserAsync(UserData user)
    {
        // 1) Si todavía no tiene ID único, se lo pedimos a Firebase.
        if (user.userId <= 0)
        {
            bool assigned = await AssignUniqueUserIdAsync(user);
            if (!assigned) return; // se reintenta en el próximo flush
        }

        if (string.IsNullOrEmpty(user.ownerUid))
            user.ownerUid = authUid;

        // 2) Se marca como limpio ANTES de subir: si cambia mientras se sube, vuelve a quedar sucio.
        dirtyUsers.Remove(user);

        string json = JsonUtility.ToJson(user);
        bool ok = await backend.SetUserJsonAsync(user.userId, json);

        if (ok)
            Debug.Log($"[Analytics] Usuario {user.userId} subido a Firebase.");
        else
            dirtyUsers.Add(user); // reintenta en el próximo flush
    }

    /// <summary>
    /// Pide el siguiente número libre a Firebase con una transacción atómica.
    /// Dos equipos jamás reciben el mismo número, así no se repite ningún ID.
    /// </summary>
    private async Task<bool> AssignUniqueUserIdAsync(UserData user)
    {
        int id = await backend.GetNextUserIdAsync();
        if (id <= 0) return false;

        user.userId = id;
        Debug.Log($"[Analytics] ID asignado: {user.userId}");
        return true;
    }

    // =====================================================
    //  RECOPILACIÓN DE INFORMACIÓN
    // =====================================================

    /// <summary>
    /// Llamar cuando el player pierde la partida.
    /// </summary>
    public void RegisterDefeat(string stationName)
    {
        if (!HasCurrentUser()) return;

        CurrentUser.defeats++;
        AddRunResult(false, stationName);
        AddStationDefeat(stationName);

        SaveUserData();
        _ = FlushAllAsync(); // momento importante: se sube ya
    }

    /// <summary>
    /// Llamar cuando el player gana la partida.
    /// </summary>
    public void RegisterWin(string stationName)
    {
        if (!HasCurrentUser()) return;

        CurrentUser.wins++;
        AddRunResult(true, stationName);

        SaveUserData();
        _ = FlushAllAsync();
    }

    /// <summary>
    /// Llamar cuando el player inicia una partida.
    /// </summary>
    public void RegisterUserRunPlay()
    {
        if (!HasCurrentUser()) return;

        CurrentUser.runsPlay++;

        CurrentUser.enemyKillsPerRun.Add(new RunEnemyKillsEntry { runPlay = CurrentUser.runsPlay });
        CurrentUser.wagonManagerPerRun.Add(new RunWagonManagerEntry { runPlay = CurrentUser.runsPlay });
        CurrentUser.locomotiveUpgradesPerRun.Add(new RunUpgradesEntry { runPlay = CurrentUser.runsPlay });
        CurrentUser.wagonsBetweenStations.Add(new RunStationWagonsEntry { runPlay = CurrentUser.runsPlay });
        CurrentUser.weaponsPerRun.Add(new RunWeaponsEntry { runPlay = CurrentUser.runsPlay });
        CurrentUser.goldPerRun.Add(new RunGoldEntry { runPlay = CurrentUser.runsPlay });

        SaveUserData();
    }

    // --- Datos globales: se retomarán más adelante. Se dejan vacíos para no romper las llamadas existentes. ---

    /// <summary>Llamar cuando un vagón es golpeado (global: pendiente).</summary>
    public void RegisterWagonHit(int index) { }

    /// <summary>Llamar cuando un vagón es destruido (global: pendiente).</summary>
    public void RegisterWagonDestroy(int index) { }

    // Guarda el resultado de la partida actual (el número de partida es runsPlay).
    private void AddRunResult(bool isWin, string stationName)
    {
        int runNumber = CurrentUser.runsPlay;

        string station = string.IsNullOrEmpty(stationName) ? "unknown" : stationName;

        // Si por error se registra dos veces la misma partida, se reemplaza en vez de duplicar.
        RunResultsEntry existing = CurrentUser.runResults.Find(r => r.runPlay == runNumber);

        if (existing != null)
        {
            existing.result = isWin;
            existing.lastStationName = station;
            return;
        }

        CurrentUser.runResults.Add(new RunResultsEntry
        {
            runPlay = runNumber,
            result = isWin,
            lastStationName = station
        });
    }

    private void AddStationDefeat(string stationName)
    {
        StationDefeatEntry entry = CurrentUser.stationDefeats.Find(s => s.stationName == stationName);

        if (entry != null)
        {
            entry.defeats++;
        }
        else
        {
            CurrentUser.stationDefeats.Add(new StationDefeatEntry { stationName = stationName, defeats = 1 });
        }
    }

    /// <summary>
    /// Llamar cuando muere un enemigo.
    /// </summary>
    public void RegisterEnemyDead(string enemyName)
    {
        if (!HasCurrentUser()) return;

        AddEnemyKill(CurrentUser.enemyKillsTotal, enemyName);

        if (CurrentUser.runsPlay > 0)
        {
            RunEnemyKillsEntry runEntry = CurrentUser.enemyKillsPerRun
                .Find(r => r.runPlay == CurrentUser.runsPlay);

            if (runEntry == null)
            {
                runEntry = new RunEnemyKillsEntry { runPlay = CurrentUser.runsPlay };
                CurrentUser.enemyKillsPerRun.Add(runEntry);
            }

            AddEnemyKill(runEntry.enemies, enemyName);
        }

        SaveUserData();
    }

    private void AddEnemyKill(List<EnemyKillEntry> list, string enemyName)
    {
        EnemyKillEntry entry = list.Find(e => e.enemyName == enemyName);

        if (entry != null)
        {
            entry.kills++;
        }
        else
        {
            list.Add(new EnemyKillEntry { enemyName = enemyName, kills = 1 });
        }
    }

    public void RegisterWagonManagerInteract()
    {
        if (!HasCurrentUser()) return;

        CurrentUser.wagonManagerTotal.managerClick++;

        RunWagonManagerEntry run = GetCurrentRunWagonManagerEntry();
        if (run != null) run.interactions.managerClick++;

        SaveUserData();
    }

    public void RegisterMoveInteract()
    {
        if (!HasCurrentUser()) return;

        CurrentUser.wagonManagerTotal.moveClick++;

        RunWagonManagerEntry run = GetCurrentRunWagonManagerEntry();
        if (run != null) run.interactions.moveClick++;

        SaveUserData();
    }

    public void RegisterSellInteract()
    {
        if (!HasCurrentUser()) return;

        CurrentUser.wagonManagerTotal.sellClick++;

        RunWagonManagerEntry run = GetCurrentRunWagonManagerEntry();
        if (run != null) run.interactions.sellClick++;

        SaveUserData();
    }

    public void RegisterUpgradeInteract()
    {
        if (!HasCurrentUser()) return;

        CurrentUser.wagonManagerTotal.upgradesClick++;

        RunWagonManagerEntry run = GetCurrentRunWagonManagerEntry();
        if (run != null) run.interactions.upgradesClick++;

        SaveUserData();
    }

    private RunWagonManagerEntry GetCurrentRunWagonManagerEntry()
    {
        if (CurrentUser.runsPlay <= 0) return null;

        RunWagonManagerEntry run = CurrentUser.wagonManagerPerRun
            .Find(r => r.runPlay == CurrentUser.runsPlay);

        if (run == null)
        {
            run = new RunWagonManagerEntry { runPlay = CurrentUser.runsPlay };
            CurrentUser.wagonManagerPerRun.Add(run);
        }

        return run;
    }

    /// <summary>
    /// Llamar cuando el jugador consigue una mejora de locomotora.
    /// </summary>
    public void RegisterLocmotiveUpgradeInRun(StatType statType, string stationName)
    {
        if (!HasCurrentUser()) return;
        if (!IsTrackedUpgrade(statType)) return;

        RunUpgradesEntry run = GetCurrentRunUpgradesEntry();
        if (run == null) return;

        StationUpgradesEntry station = run.stations.Find(s => s.stationName == stationName);

        // Estación nueva en esta run: arranca con las mejoras que ya tenía el jugador.
        if (station == null)
        {
            station = new StationUpgradesEntry { stationName = stationName };

            if (run.stations.Count > 0)
                station.upgrades = run.stations[run.stations.Count - 1].upgrades; // copia del struct

            run.stations.Add(station);
        }

        ApplyUpgrade(ref station.upgrades, statType);

        SaveUserData();
    }

    private bool IsTrackedUpgrade(StatType statType)
    {
        switch (statType)
        {
            case StatType.MaxHp:
            case StatType.Defense:
            case StatType.GoldMultiplier:
            case StatType.DamageMultiplier:
            case StatType.AttackSpeed:
                return true;
            default:
                return false;
        }
    }

    private void ApplyUpgrade(ref LocomotiveUpgrades upgrades, StatType statType)
    {
        switch (statType)
        {
            case StatType.MaxHp: upgrades.maxHPUpgrade++; break;
            case StatType.Defense: upgrades.defenseUpgrade++; break;
            case StatType.GoldMultiplier: upgrades.goldUpgrade++; break;
            case StatType.DamageMultiplier: upgrades.damageUpgrade++; break;
            case StatType.AttackSpeed: upgrades.rofUpgrade++; break;
        }
    }

    private RunUpgradesEntry GetCurrentRunUpgradesEntry()
    {
        if (CurrentUser.runsPlay <= 0) return null;

        RunUpgradesEntry run = CurrentUser.locomotiveUpgradesPerRun
            .Find(r => r.runPlay == CurrentUser.runsPlay);

        if (run == null)
        {
            run = new RunUpgradesEntry { runPlay = CurrentUser.runsPlay };
            CurrentUser.locomotiveUpgradesPerRun.Add(run);
        }

        return run;
    }

    // ---------- Al LLEGAR a una estación ----------

    public void RegisterWagonOnArrival(string stationName, string wagonName)
    {
        RegisterWagonsOnArrival(stationName, new[] { wagonName });
    }

    public void RegisterWagonsOnArrival(string stationName, IEnumerable<string> wagonNames)
    {
        if (!HasCurrentUser()) return;

        StationWagonsEntry station = GetCurrentStationWagonsEntry(stationName);
        if (station == null) return;

        foreach (string wagonName in wagonNames)
            AddWagonType(station.arrivalWagons, wagonName);

        SaveUserData();
    }

    // ---------- Al IRSE de una estación ----------

    public void RegisterWagonOnDeparture(string stationName, string wagonName)
    {
        RegisterWagonsOnDeparture(stationName, new[] { wagonName });
    }

    public void RegisterWagonsOnDeparture(string stationName, IEnumerable<string> wagonNames)
    {
        if (!HasCurrentUser()) return;

        StationWagonsEntry station = GetCurrentStationWagonsEntry(stationName);
        if (station == null) return;

        foreach (string wagonName in wagonNames)
            AddWagonType(station.departureWagons, wagonName);

        SaveUserData();
    }

    private void AddWagonType(List<WagonTypeCountEntry> list, string wagonName)
    {
        WagonTypeCountEntry entry = list.Find(w => w.wagonName == wagonName);

        if (entry != null)
        {
            entry.count++;
        }
        else
        {
            list.Add(new WagonTypeCountEntry { wagonName = wagonName, count = 1 });
        }
    }

    private StationWagonsEntry GetCurrentStationWagonsEntry(string stationName)
    {
        if (CurrentUser.runsPlay <= 0) return null;

        RunStationWagonsEntry run = CurrentUser.wagonsBetweenStations
            .Find(r => r.runPlay == CurrentUser.runsPlay);

        if (run == null)
        {
            run = new RunStationWagonsEntry { runPlay = CurrentUser.runsPlay };
            CurrentUser.wagonsBetweenStations.Add(run);
        }

        StationWagonsEntry station = run.stations.Find(s => s.stationName == stationName);

        if (station == null)
        {
            station = new StationWagonsEntry { stationName = stationName };
            run.stations.Add(station);
        }

        return station;
    }

    /// <summary>
    /// Llamar cuando el jugador compra un arma.
    /// </summary>
    public void RegisterOwnedWeapon(string stationName, string weaponName)
    {
        if (!HasCurrentUser()) return;
        if (CurrentUser.runsPlay <= 0) return;

        RunWeaponsEntry run = CurrentUser.weaponsPerRun
            .Find(r => r.runPlay == CurrentUser.runsPlay);

        if (run == null)
        {
            run = new RunWeaponsEntry { runPlay = CurrentUser.runsPlay };
            CurrentUser.weaponsPerRun.Add(run);
        }

        StationWeaponEntry station = run.stations.Find(s => s.stationName == stationName);

        if (station != null)
        {
            station.weaponName = weaponName;
        }
        else
        {
            run.stations.Add(new StationWeaponEntry { stationName = stationName, weaponName = weaponName });
        }

        SaveUserData();
    }

    /// <summary>
    /// Llamar al ENTRAR a una estación, con el oro que tiene el jugador.
    /// </summary>
    public void RegisterGoldInStationOnArrival(string stationName, int gold)
    {
        if (!HasCurrentUser()) return;

        StationGoldEntry station = GetCurrentStationGoldEntry(stationName);
        if (station == null) return;

        station.goldOnArrival = gold;
        station.arrivalRegistered = true;

        SaveUserData();
    }

    /// <summary>
    /// Llamar al SALIR de una estación, con el oro que tiene el jugador.
    /// </summary>
    public void RegisterGoldInStationOnDeparture(string stationName, int gold)
    {
        if (!HasCurrentUser()) return;

        StationGoldEntry station = GetCurrentStationGoldEntry(stationName);
        if (station == null) return;

        station.goldOnDeparture = gold;
        station.departureRegistered = true;

        SaveUserData();
    }

    private StationGoldEntry GetCurrentStationGoldEntry(string stationName)
    {
        if (CurrentUser.runsPlay <= 0) return null;

        RunGoldEntry run = CurrentUser.goldPerRun
            .Find(r => r.runPlay == CurrentUser.runsPlay);

        if (run == null)
        {
            run = new RunGoldEntry { runPlay = CurrentUser.runsPlay };
            CurrentUser.goldPerRun.Add(run);
        }

        StationGoldEntry station = run.stations.Find(s => s.stationName == stationName);

        if (station == null)
        {
            station = new StationGoldEntry { stationName = stationName };
            run.stations.Add(station);
        }

        return station;
    }
}