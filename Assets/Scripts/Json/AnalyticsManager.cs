using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;


// =========================================================
//  MANAGER
// =========================================================

public class AnalyticsManager : MonoBehaviour
{
    // ----------------- Singleton -----------------
    public static AnalyticsManager Instance { get; private set; }

    // ----------------- Datos globales en memoria -----------------
    public int PlayerDefeat { get; private set; } = 0;
    public int PlayerWin { get; private set; } = 0;

    public Dictionary<int, int> WagonInLocomotiveHit = new Dictionary<int, int>();
    public Dictionary<int, int> WagonInLocomotiveDestroy = new Dictionary<int, int>();

    // ----------------- Datos de usuarios en memoria -----------------
    private const int MinUserId = 1;
    private const int MaxUserId = 1000;

    // Todos los usuarios que jugaron en este equipo.
    private readonly List<UserData> users = new List<UserData>();
    public IReadOnlyList<UserData> Users => users;

    // Usuario que está jugando ahora (se actualiza mientras se juega).
    public UserData CurrentUser { get; private set; }

    // ----------------- Archivos -----------------
    private const string GlobalFileName = "analytics_globalData.json";
    private const string UserFileName = "analytics_usersData.json";

    private string GlobalFilePath => Path.Combine(Application.persistentDataPath, GlobalFileName);
    private string UserFilePath => Path.Combine(Application.persistentDataPath, UserFileName);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Al iniciar, cargamos lo que ya estaba guardado.
        LoadGlobalData();
        LoadUserData();

        // Cada vez que se abre el juego arranca con un usuario nuevo.
        CreateNewUser();
    }


    // =====================================================
    //  FUNCIONES DE CONVERSION 
    // =====================================================

    // Dictionary -> lista para el JSON (ordenada por índice para que se lea bien)
    private List<WagonCountEntry> DictionaryToList(Dictionary<int, int> dictionary)
    {
        List<WagonCountEntry> list = new List<WagonCountEntry>();

        List<int> keys = new List<int>(dictionary.Keys);
        keys.Sort();

        foreach (int key in keys)
        {
            list.Add(new WagonCountEntry { wagonIndex = key, count = dictionary[key] });
        }

        return list;
    }

    // Lista del JSON -> Dictionary
    private void ListToDictionary(List<WagonCountEntry> list, Dictionary<int, int> dictionary)
    {
        dictionary.Clear();

        if (list == null) return;

        foreach (WagonCountEntry entry in list)
        {
            dictionary[entry.wagonIndex] = entry.count;
        }
    }

    // Dictionary -> una línea de texto por elemento, para la parte legible
    private List<string> BuildWagonTexts(Dictionary<int, int> dictionary, string unit)
    {
        List<string> texts = new List<string>();

        List<int> keys = new List<int>(dictionary.Keys);
        keys.Sort();

        foreach (int key in keys)
        {
            texts.Add("Vagón " + key + " -> " + dictionary[key] + " " + unit);
        }

        return texts;
    }

    private List<string> BuildRunResultsTexts(List<RunResultsEntry> results)
    {
        List<string> texts = new List<string>();

        foreach (RunResultsEntry entry in results)
        {
            texts.Add("-Partida " + entry.runPlay + " : " + (entry.result ? "victoria" : "derrota"));
        }

        return texts;
    }

    private List<string> BuildStationDefeatsTexts(List<StationDefeatEntry> entries)
    {
        List<string> texts = new List<string>();

        foreach (StationDefeatEntry entry in entries)
        {
            texts.Add("-" + entry.stationName + ": " + entry.defeats);
        }

        return texts;
    }

    private List<string> BuildEnemyKillsTexts(List<EnemyKillEntry> entries)
    {
        List<string> texts = new List<string>();

        foreach (EnemyKillEntry entry in entries)
        {
            texts.Add("-" + entry.enemyName + ": " + entry.kills);
        }

        return texts;
    }

    // Las 4 líneas de un struct
    private List<string> BuildWagonManagerTexts(WagonManagerInteraction data)
    {
        return new List<string>
    {
        "-Manage Wagon = " + data.managerClick,
        "-Move = " + data.moveClick,
        "-Sell = " + data.sellClick,
        "-Upgrade = " + data.upgradesClick
    };
    }

    // "Run 1" y debajo sus 4 líneas, para cada run
    private List<string> BuildWagonManagerPerRunTexts(List<RunWagonManagerEntry> runs)
    {
        List<string> texts = new List<string>();

        foreach (RunWagonManagerEntry run in runs)
        {
            texts.Add("Run " + run.runPlay);
            texts.AddRange(BuildWagonManagerTexts(run.interactions));
        }

        return texts;
    }

    // "-Run 1" y debajo "--Goblin: 3"
    private List<string> BuildEnemyKillsPerRunTexts(List<RunEnemyKillsEntry> runs)
    {
        List<string> texts = new List<string>();

        foreach (RunEnemyKillsEntry run in runs)
        {
            texts.Add("-Run " + run.runPlay);

            foreach (EnemyKillEntry entry in run.enemies)
            {
                texts.Add("--" + entry.enemyName + ": " + entry.kills);
            }
        }

        return texts;
    }

    private List<string> BuildLocomotiveUpgradesTexts(List<RunUpgradesEntry> runs)
    {
        List<string> texts = new List<string>();

        foreach (RunUpgradesEntry run in runs)
        {
            texts.Add("Partida " + run.runPlay);

            foreach (StationUpgradesEntry station in run.stations)
            {
                texts.Add("-" + station.stationName);
                texts.Add("--Vida Maxima = " + station.upgrades.maxHPUpgrade);
                texts.Add("--Placas = " + station.upgrades.defenseUpgrade);
                texts.Add("--Vagon de oro = " + station.upgrades.goldUpgrade);
                texts.Add("--Mejora de daño = " + station.upgrades.damageUpgrade);
                texts.Add("--Mejora de cadencia = " + station.upgrades.rofUpgrade);
            }
        }

        return texts;
    }

    private List<string> BuildWagonsBetweenStationsTexts(List<RunStationWagonsEntry> runs)
    {
        List<string> texts = new List<string>();

        foreach (RunStationWagonsEntry run in runs)
        {
            texts.Add("Partida " + run.runPlay);

            foreach (StationWagonsEntry station in run.stations)
            {
                texts.Add("Estacion " + station.stationName);

                texts.Add("Vagones al llegar");
                texts.AddRange(BuildWagonTypeTexts(station.arrivalWagons));

                texts.Add("Vagones al irse");
                texts.AddRange(BuildWagonTypeTexts(station.departureWagons));
            }
        }

        return texts;
    }

    // "-Cañon: 3"
    private List<string> BuildWagonTypeTexts(List<WagonTypeCountEntry> entries)
    {
        List<string> texts = new List<string>();

        if (entries.Count == 0)
        {
            texts.Add("tren sin vagones");
            return texts;
        }

        foreach (WagonTypeCountEntry entry in entries)
        {
            texts.Add("-" + entry.wagonName + ": " + entry.count);
        }

        return texts;
    }

    private List<string> BuildWeaponsTexts(List<RunWeaponsEntry> runs)
    {
        List<string> texts = new List<string>();

        foreach (RunWeaponsEntry run in runs)
        {
            texts.Add("Partida " + run.runPlay);

            if (run.stations.Count == 0)
            {
                texts.Add("El jugador no compro armas nuevas");
                continue;
            }

            foreach (StationWeaponEntry station in run.stations)
            {
                texts.Add("Estacion " + station.stationName);
                texts.Add("-" + station.weaponName);
            }
        }

        return texts;
    }

    private List<string> BuildGoldTexts(List<RunGoldEntry> runs)
    {
        List<string> texts = new List<string>();

        foreach (RunGoldEntry run in runs)
        {
            texts.Add("Partida " + run.runPlay);

            foreach (StationGoldEntry station in run.stations)
            {
                texts.Add("Estacion " + station.stationName);

                texts.Add("-Oro al entrar: " +
                    (station.arrivalRegistered ? station.goldOnArrival.ToString() : "sin registrar"));

                texts.Add("-Oro al salir: " +
                    (station.departureRegistered ? station.goldOnDeparture.ToString() : "sin registrar"));
            }
        }

        return texts;
    }

    // =====================================================
    //  RECOPILACIÓN DE INFORMACIÓN
    // =====================================================

    /// <summary>
    /// Llamar cuando el player pierde la partida.
    /// Suma la derrota al global y al usuario actual.
    /// </summary>
    public void RegisterDefeat(string stationName)
    {
        PlayerDefeat++;
        SaveGlobalData();

        if (!HasCurrentUser()) return;

        CurrentUser.defeats++;
        AddRunResult(false);
        AddStationDefeat(stationName);

        SaveUserData();
    }

    /// <summary>
    /// Llamar cuando el player gana la partida.
    /// Suma la victoria al global y al usuario actual.
    /// </summary>
    public void RegisterWin()
    {
        PlayerWin++;
        SaveGlobalData();

        if (!HasCurrentUser()) return;

        CurrentUser.wins++;
        AddRunResult(true);

        SaveUserData();
    }

    /// <summary>
    /// Llamar cuando el player inicia una partida
    /// Suma la cantidad de veces que el jugador jugo una partida.
    /// </summary>
    public void RegisterUserRunPlay()
    {
        if (!HasCurrentUser()) return;

        CurrentUser.runsPlay++;

        // Nueva run -> se agrega un elemento al "diccionario" de enemigos por run.
        CurrentUser.enemyKillsPerRun.Add(new RunEnemyKillsEntry { runPlay = CurrentUser.runsPlay });
        CurrentUser.wagonManagerPerRun.Add(new RunWagonManagerEntry { runPlay = CurrentUser.runsPlay });
        CurrentUser.locomotiveUpgradesPerRun.Add(new RunUpgradesEntry { runPlay = CurrentUser.runsPlay });
        CurrentUser.wagonsBetweenStations.Add(new RunStationWagonsEntry { runPlay = CurrentUser.runsPlay });
        CurrentUser.weaponsPerRun.Add(new RunWeaponsEntry { runPlay = CurrentUser.runsPlay });
        CurrentUser.goldPerRun.Add(new RunGoldEntry { runPlay = CurrentUser.runsPlay });

        SaveUserData();
    }

    //Llamar funcion en Take Damage de Wagon

    /// <summary>
    /// Llamar cuando un vagon es golpeado
    /// Suma la cantidad de veces que un vagon fue golpeado
    /// </summary>
    public void RegisterWagonHit(int index)
    {
        if (WagonInLocomotiveHit.ContainsKey(index))
        {
            WagonInLocomotiveHit[index]++;
        }
        else
        {
            WagonInLocomotiveHit.Add(index, 1);
        }

        SaveGlobalData();
    }

    //Llamar funcion en Break de Wagon

    /// <summary>
    /// Llamar cuando un vagon es golpeado
    /// Suma la cantidad de veces que un vagon fue destruido
    /// </summary>
    public void RegisterWagonDestroy(int index)
    {
        if (WagonInLocomotiveDestroy.ContainsKey(index))
        {
            WagonInLocomotiveDestroy[index]++;
        }
        else
        {
            WagonInLocomotiveDestroy.Add(index, 1);
        }

        SaveGlobalData();
    }

    // Guarda el resultado de la partida actual (el número de partida es runsPlay).
    private void AddRunResult(bool isWin)
    {
        int runNumber = CurrentUser.runsPlay;

        // Si por error se registra dos veces la misma partida, se reemplaza en vez de duplicar.
        RunResultsEntry existing = CurrentUser.runResults.Find(r => r.runPlay == runNumber);

        if (existing != null)
        {
            existing.result = isWin;
            return;
        }

        CurrentUser.runResults.Add(new RunResultsEntry { runPlay = runNumber, result = isWin });
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

    //Llamar funcion cuando muere un enemigo 

    /// <summary>
    /// Llamar cuando muere un enemigo.
    /// Lo suma al total del juego y a la run actual del usuario actual.
    /// </summary>
    public void RegisterEnemyDead(string enemyName)
    {
        if (!HasCurrentUser()) return;

        // Total durante todo el juego
        AddEnemyKill(CurrentUser.enemyKillsTotal, enemyName);

        // Run actual (solo si ya empezó una run)
        if (CurrentUser.runsPlay > 0)
        {
            RunEnemyKillsEntry runEntry = CurrentUser.enemyKillsPerRun
                .Find(r => r.runPlay == CurrentUser.runsPlay);

            // Por si la entrada de la run no existe, se crea.
            if (runEntry == null)
            {
                runEntry = new RunEnemyKillsEntry { runPlay = CurrentUser.runsPlay };
                CurrentUser.enemyKillsPerRun.Add(runEntry);
            }

            AddEnemyKill(runEntry.enemies, enemyName);
        }

        SaveUserData();
    }

    // Suma una muerte al enemigo (si no está en la lista, lo agrega con 1).
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

    // Devuelve la entrada de la run actual (null si todavía no empezó ninguna run).
    private RunWagonManagerEntry GetCurrentRunWagonManagerEntry()
    {
        if (CurrentUser.runsPlay <= 0) return null;

        RunWagonManagerEntry run = CurrentUser.wagonManagerPerRun
            .Find(r => r.runPlay == CurrentUser.runsPlay);

        // Por si la entrada de la run no existe, se crea.
        if (run == null)
        {
            run = new RunWagonManagerEntry { runPlay = CurrentUser.runsPlay };
            CurrentUser.wagonManagerPerRun.Add(run);
        }

        return run;
    }

    /// <summary>
    /// Llamar cuando el jugador consigue una mejora de locomotora.
    /// Suma 1 al valor correspondiente en la estación indicada, dentro de la run actual.
    /// </summary>
    public void RegisterLocmotiveUpgradeInRun(StatType statType, string stationName)
    {
        if (!HasCurrentUser()) return;

        // Solo se registran los 5 tipos que importan (FuelOptimizer y Speed se ignoran).
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

    // Suma 1 al campo del struct que corresponde al tipo de mejora.
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

    // Devuelve la entrada de la run actual (null si todavía no empezó ninguna run).
    private RunUpgradesEntry GetCurrentRunUpgradesEntry()
    {
        if (CurrentUser.runsPlay <= 0) return null;

        RunUpgradesEntry run = CurrentUser.locomotiveUpgradesPerRun
            .Find(r => r.runPlay == CurrentUser.runsPlay);

        // Por si la entrada de la run no existe, se crea.
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

    // Suma +1 al vagón (si no está en la lista, lo agrega con 1).
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

    // Devuelve la estación dentro de la run actual (la crea si no existe).
    // Devuelve null si todavía no empezó ninguna run.
    private StationWagonsEntry GetCurrentStationWagonsEntry(string stationName)
    {
        if (CurrentUser.runsPlay <= 0) return null;

        RunStationWagonsEntry run = CurrentUser.wagonsBetweenStations
            .Find(r => r.runPlay == CurrentUser.runsPlay);

        // Por si la entrada de la run no existe, se crea.
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

        // Por si la entrada de la run no existe, se crea.
        if (run == null)
        {
            run = new RunWeaponsEntry { runPlay = CurrentUser.runsPlay };
            CurrentUser.weaponsPerRun.Add(run);
        }

        StationWeaponEntry station = run.stations.Find(s => s.stationName == stationName);

        if (station != null)
        {
            // Como en un Dictionary<string, string>: la misma key reemplaza el valor.
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

    // Devuelve la estación dentro de la run actual (la crea si no existe).
    // Devuelve null si todavía no empezó ninguna run.
    private StationGoldEntry GetCurrentStationGoldEntry(string stationName)
    {
        if (CurrentUser.runsPlay <= 0) return null;

        RunGoldEntry run = CurrentUser.goldPerRun
            .Find(r => r.runPlay == CurrentUser.runsPlay);

        // Por si la entrada de la run no existe, se crea.
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

    // =====================================================
    //  JSON GLOBAL: GUARDAR / CARGAR / BORRAR
    // =====================================================

    /// <summary>
    /// Escribe (o edita) el JSON global con los valores actuales de la clase.
    /// </summary>
    public void SaveGlobalData()
    {
        GlobalAnalyticsFile file = new GlobalAnalyticsFile();

        FillGlobalFile(file);

        string json = JsonUtility.ToJson(file, true); // true = formato indentado
        File.WriteAllText(GlobalFilePath, json);

        Debug.Log($"[Analytics] Datos globales guardados en: {GlobalFilePath}");
    }

    /// <summary>
    /// Lee el JSON global y carga los valores en esta clase.
    /// Si el archivo no existe, lo crea con valores en 0.
    /// </summary>
    public void LoadGlobalData()
    {
        if (!File.Exists(GlobalFilePath))
        {
            SaveGlobalData();
            return;
        }

        GlobalAnalyticsFile file = null;

        try
        {
            string json = File.ReadAllText(GlobalFilePath);
            file = JsonUtility.FromJson<GlobalAnalyticsFile>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[Analytics] Error leyendo JSON global: {e.Message}");
        }

        if (file == null || file.Datos == null)
        {
            Debug.LogWarning("[Analytics] JSON global inválido, se reinicia.");
            ClearGlobalData();
            return;
        }

        RestoreGlobalData(file);
    }

    /// <summary>
    /// Borra la información global acumulada y deja el JSON limpio.
    /// </summary>
    public void ClearGlobalData()
    {
        PlayerDefeat = 0;
        PlayerWin = 0;
        
        WagonInLocomotiveHit.Clear();
        WagonInLocomotiveDestroy.Clear();

        SaveGlobalData();
        Debug.Log("[Analytics] Datos globales borrados.");
    }

    // Pasa los valores de la clase -> archivo
    private void FillGlobalFile(GlobalAnalyticsFile file)
    {
        // Sección para el código
        file.Datos.playerDefeatData = PlayerDefeat;
        file.Datos.playerWinData = PlayerWin;

        file.Datos.wagonInLocomotiveHit = DictionaryToList(WagonInLocomotiveHit);
        file.Datos.wagonInLocomotiveDestroy = DictionaryToList(WagonInLocomotiveDestroy);

        // Sección para humanos
        file.Resumen.playerDefeatText = "Total de derrotas: " + PlayerDefeat;
        file.Resumen.playerWinText = "Total de victorias: " + PlayerWin;

        file.Resumen.wagonHitText = BuildWagonTexts(WagonInLocomotiveHit, "golpes");
        file.Resumen.wagonDestroyText = BuildWagonTexts(WagonInLocomotiveDestroy, "destrucciones");
    }

    // Pasa los valores del archivo -> clase
    private void RestoreGlobalData(GlobalAnalyticsFile file)
    {
        PlayerDefeat = file.Datos.playerDefeatData;
        PlayerWin = file.Datos.playerWinData;

        ListToDictionary(file.Datos.wagonInLocomotiveHit, WagonInLocomotiveHit);
        ListToDictionary(file.Datos.wagonInLocomotiveDestroy, WagonInLocomotiveDestroy);

        Debug.Log($"[Analytics] Datos globales cargados. Derrotas: {PlayerDefeat} | Victorias: {PlayerWin}");
    }

    // =====================================================
    //  USUARIOS: CREAR
    // =====================================================

    /// <summary>
    /// Crea un usuario nuevo con un número libre entre 1 y 1000 y lo deja como CurrentUser.
    /// Sus variables empiezan en 0.
    /// El usuario anterior queda guardado en la lista con su información.
    /// Devuelve null si ya están ocupados los 1000 números.
    /// </summary>
    public UserData CreateNewUser()
    {
        // Armamos la lista de números que todavía no se usaron.
        List<int> availableIds = new List<int>();
        for (int id = MinUserId; id <= MaxUserId; id++)
        {
            if (!users.Exists(u => u.userId == id))
                availableIds.Add(id);
        }

        if (availableIds.Count == 0)
        {
            Debug.LogWarning("[Analytics] No quedan nombres de usuario disponibles (1-1000).");
            return null;
        }

        int newId = availableIds[Random.Range(0, availableIds.Count)];

        // Usuario nuevo: todas sus variables arrancan en 0.
        UserData newUser = new UserData
        {
            userId = newId,
            wins = 0,
            defeats = 0,
            runsPlay = 0,
        };

        users.Add(newUser);
        CurrentUser = newUser;

        SaveUserData();
        Debug.Log($"[Analytics] Usuario nuevo creado: {newId}");

        return newUser;
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
    //  JSON DE USUARIOS: GUARDAR / CARGAR / BORRAR
    // =====================================================

    /// <summary>
    /// Escribe (o edita) el JSON de usuarios con la lista actual.
    /// </summary>
    public void SaveUserData()
    {
        UserAnalyticsFile file = new UserAnalyticsFile();

        FillUserFile(file);

        string json = JsonUtility.ToJson(file, true);
        json = AddReadableSpacing(json);
        File.WriteAllText(UserFilePath, json);

        Debug.Log($"[Analytics] Datos de usuarios guardados en: {UserFilePath}");
    }

    /// <summary>
    /// Agrega líneas en blanco al JSON de usuarios para que el resumen se lea mejor:
    /// una después de "userText" y otra antes del separador.
    /// Las líneas en blanco son espacios en blanco válidos en JSON, no afectan la carga.
    /// </summary>
    private string AddReadableSpacing(string json)
    {
        // Línea en blanco después de "userText": "...",
        json = Regex.Replace(json, "(\"userText\": \"[^\"]*\",)(\\r?\\n)", "$1$2$2");

        // Línea en blanco antes de "separatorText"
        json = Regex.Replace(json, "(\\r?\\n)([ \\t]*\"separatorText\")", "$1$1$2");

        // Línea en blanco antes de cada título de sección
        json = Regex.Replace(json,
            "(\\r?\\n)([ \\t]*\"(runResultsTitle|stationDefeatsTitle|enemyKillsTitle|enemyKillsPerRunTitle|wagonManagerTitle|locomotiveUpgradesTitle|wagonsBetweenStationsTitle|weaponsTitle|goldTitle)\")",
            "$1$1$2");

        // Línea en blanco entre "Partida N" y su primera estación
        json = Regex.Replace(json, "(\"Partida \\d+\",)(\\r?\\n)", "$1$2$2");

        // Línea en blanco antes de cada "Partida N" que no sea la primera
        json = Regex.Replace(json, "(?<=\",)(\\r?\\n)([ \\t]*\"Partida \\d+\")", "$1$1$2");

        // Vagones: línea en blanco entre "Estacion ..." y "Vagones al llegar"
        json = Regex.Replace(json,
            "^([ \\t]*\"Estacion [^\"]*\",)(\\r?\\n)(?=[ \\t]*\"Vagones al llegar\")",
            "$1$2$2", RegexOptions.Multiline);

        // Oro: línea en blanco entre una estación y la siguiente
        json = Regex.Replace(json,
            "(\"-Oro al salir: [^\"]*\",)(\\r?\\n)([ \\t]*\"Estacion )",
            "$1$2$2$3");

        return json;
    }

    /// <summary>
    /// Lee el JSON de usuarios y carga la lista en esta clase.
    /// Si el archivo no existe, lo crea vacío.
    /// </summary>
    public void LoadUserData()
    {
        if (!File.Exists(UserFilePath))
        {
            SaveUserData();
            return;
        }

        UserAnalyticsFile file = null;

        try
        {
            string json = File.ReadAllText(UserFilePath);
            file = JsonUtility.FromJson<UserAnalyticsFile>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[Analytics] Error leyendo JSON de usuarios: {e.Message}");
        }

        if (file == null || file.Datos == null || file.Datos.users == null)
        {
            Debug.LogWarning("[Analytics] JSON de usuarios inválido, se reinicia.");
            ClearUserData();
            return;
        }

        RestoreUserData(file);
    }

    /// <summary>
    /// Borra todos los usuarios y deja el JSON de usuarios limpio.
    /// </summary>
    public void ClearUserData()
    {
        users.Clear();
        CurrentUser = null;
        SaveUserData();
        Debug.Log("[Analytics] Datos de usuarios borrados.");
    }

    // Pasa la lista de la clase -> archivo
    private void FillUserFile(UserAnalyticsFile file)
    {
        // Sección para el código
        file.Datos.users = new List<UserData>(users);

        // Sección para humanos
        file.Resumen.totalUsersText = "Total de usuarios registrados: " + users.Count;
        file.Resumen.usersSummary.Clear();

        foreach (UserData user in users)
        {
            file.Resumen.usersSummary.Add(new UserSummaryEntry
            {
                userText = "Usuario " + user.userId,

                runPlayText = "Cantidad de partidas jugadas: " + user.runsPlay,
                defeatsText = "Cantidad de derrotas: " + user.defeats,
                winsText = "Cantidad de victorias: " + user.wins,
                runResultsText = BuildRunResultsTexts(user.runResults),
                stationDefeatsText = BuildStationDefeatsTexts(user.stationDefeats),
                enemyKillsRunsPlayedText = "Total de partidas jugadas: " + user.runsPlay,
                enemyKillsTotalText = BuildEnemyKillsTexts(user.enemyKillsTotal),
                enemyKillsPerRunText = BuildEnemyKillsPerRunTexts(user.enemyKillsPerRun),
                wagonManagerTotalText = BuildWagonManagerTexts(user.wagonManagerTotal),
                wagonManagerPerRunText = BuildWagonManagerPerRunTexts(user.wagonManagerPerRun),
                locomotiveUpgradesText = BuildLocomotiveUpgradesTexts(user.locomotiveUpgradesPerRun),
                wagonsBetweenStationsText = BuildWagonsBetweenStationsTexts(user.wagonsBetweenStations),
                weaponsText = BuildWeaponsTexts(user.weaponsPerRun),
                goldText = BuildGoldTexts(user.goldPerRun),
                separatorText = "-----------------------------------------------------"
            });
        }
    }

    // Pasa la lista del archivo -> clase
    private void RestoreUserData(UserAnalyticsFile file)
    {
        // Los usuarios cargados conservan los valores guardados en el JSON.
        users.Clear();
        users.AddRange(file.Datos.users);

        // Ninguno de los cargados es el actual:
        // el usuario actual lo define CreateNewUser() (se llama en Awake).
        CurrentUser = null;

        Debug.Log($"[Analytics] Datos de usuarios cargados. Total usuarios: {users.Count}");
    }
}