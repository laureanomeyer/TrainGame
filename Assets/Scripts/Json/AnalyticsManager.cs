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
    //  RECOPILACIÓN DE INFORMACIÓN
    // =====================================================

    /// <summary>
    /// Llamar cuando el player pierde la partida.
    /// Suma la derrota al global y al usuario actual.
    /// </summary>
    public void RegisterDefeat()
    {
        PlayerDefeat++;
        SaveGlobalData();

        if (!HasCurrentUser()) return;

        CurrentUser.defeats++;
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
        SaveUserData();
    }

    /// <summary>
    /// Llamar cuando el player inicia una partida
    /// Suma la cantidad de veces que el jugador jugo una partida.
    /// </summary>
    public void RegisterUserRunPlay()
    {
        CurrentUser.runsPlay++;

        SaveUserData();
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
        SaveGlobalData();
        Debug.Log("[Analytics] Datos globales borrados.");
    }

    // Pasa los valores de la clase -> archivo
    private void FillGlobalFile(GlobalAnalyticsFile file)
    {
        // Sección para el código
        file.Datos.playerDefeatData = PlayerDefeat;
        file.Datos.playerWinData = PlayerWin;

        // Sección para humanos
        file.Resumen.playerDefeatText = "Total de derrotas: " + PlayerDefeat;
        file.Resumen.playerWinText = "Total de victorias: " + PlayerWin;
    }

    // Pasa los valores del archivo -> clase
    private void RestoreGlobalData(GlobalAnalyticsFile file)
    {
        PlayerDefeat = file.Datos.playerDefeatData;
        PlayerWin = file.Datos.playerWinData;

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
            defeats = 0
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